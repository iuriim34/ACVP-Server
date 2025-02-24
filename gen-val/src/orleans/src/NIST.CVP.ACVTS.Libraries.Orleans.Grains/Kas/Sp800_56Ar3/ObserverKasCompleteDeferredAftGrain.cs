﻿using System;
using System.Threading.Tasks;
using NIST.CVP.ACVTS.Libraries.Common;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Enums;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.FixedInfo;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.KC;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.KDA;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Scheme;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Sp800_56Ar3;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Sp800_56Ar3.Builders;
using NIST.CVP.ACVTS.Libraries.Oracle.Abstractions.ParameterTypes.Kas.Sp800_56Ar3;
using NIST.CVP.ACVTS.Libraries.Oracle.Abstractions.ResultTypes.Kas.Sp800_56Ar3;
using NIST.CVP.ACVTS.Libraries.Orleans.Grains.Interfaces.Kas.Sp800_56Ar3;


namespace NIST.CVP.ACVTS.Libraries.Orleans.Grains.Kas.Sp800_56Ar3
{
    public class ObserverKasCompleteDeferredAftGrain : ObservableOracleGrainBase<KasAftDeferredResult>,
        IObserverKasCompleteDeferredAftGrain
    {
        private readonly IKasBuilder _kasBuilder;
        private readonly ISchemeBuilder _schemeBuilder;
        private readonly ISecretKeyingMaterialBuilder _serverSecretKeyingMaterialBuilder;
        private readonly ISecretKeyingMaterialBuilder _iutSecretKeyingMaterialBuilder;
        private readonly IKdfFactory _kdfFactory;
        private readonly IKeyConfirmationFactory _keyConfirmationFactory;
        private readonly IFixedInfoFactory _fixedInfoFactory;

        private KasAftDeferredParameters _param;

        public ObserverKasCompleteDeferredAftGrain(
            LimitedConcurrencyLevelTaskScheduler nonOrleansScheduler,
            IKasBuilder kasBuilder,
            ISchemeBuilder schemeBuilder,
            ISecretKeyingMaterialBuilder serverSecretKeyingMaterialBuilder,
            ISecretKeyingMaterialBuilder iutSecretKeyingMaterialBuilder,
            IKdfFactory kdfFactory,
            IKeyConfirmationFactory keyConfirmationFactory,
            IFixedInfoFactory fixedInfoFactory
        ) : base(nonOrleansScheduler)
        {
            _kasBuilder = kasBuilder;
            _schemeBuilder = schemeBuilder;
            _serverSecretKeyingMaterialBuilder = serverSecretKeyingMaterialBuilder;
            _iutSecretKeyingMaterialBuilder = iutSecretKeyingMaterialBuilder;
            _kdfFactory = kdfFactory;
            _keyConfirmationFactory = keyConfirmationFactory;
            _fixedInfoFactory = fixedInfoFactory;
        }

        public async Task<bool> BeginWorkAsync(KasAftDeferredParameters param)
        {
            _param = param;

            await BeginGrainWorkAsync();
            return await Task.FromResult(true);
        }

        protected override async Task DoWorkAsync()
        {
            try
            {
                // depending if the server is party U or party V in the negotiation.
                var isServerPartyU = _param.ServerGenerationRequirements.ThisPartyKasRole ==
                                     KeyAgreementRole.InitiatorPartyU;
                var isServerPartyV = !isServerPartyU;

                _serverSecretKeyingMaterialBuilder
                    .WithDomainParameters(_param.DomainParameters)
                    .WithPartyId(_param.PartyIdServer)
                    .WithEphemeralKey(_param.EphemeralKeyServer)
                    .WithStaticKey(_param.StaticKeyServer)
                    .WithEphemeralNonce(_param.EphemeralNonceServer)
                    .WithDkmNonce(_param.DkmNonceServer);

                var serverSecretKeyingMaterial = _serverSecretKeyingMaterialBuilder
                    .Build(
                        _param.KasScheme,
                        _param.ServerGenerationRequirements.KasMode,
                        _param.ServerGenerationRequirements.ThisPartyKasRole,
                        _param.ServerGenerationRequirements.ThisPartyKeyConfirmationRole,
                        _param.ServerGenerationRequirements.KeyConfirmationDirection);

                _iutSecretKeyingMaterialBuilder
                    .WithDomainParameters(_param.DomainParameters)
                    .WithPartyId(_param.PartyIdIut)
                    .WithEphemeralKey(_param.EphemeralKeyIut)
                    .WithStaticKey(_param.StaticKeyIut)
                    .WithEphemeralNonce(_param.EphemeralNonceIut)
                    .WithDkmNonce(_param.DkmNonceIut);

                var iutSecretKeyingMaterial = _iutSecretKeyingMaterialBuilder
                    .Build(
                        _param.KasScheme,
                        _param.IutGenerationRequirements.KasMode,
                        _param.IutGenerationRequirements.ThisPartyKasRole,
                        _param.IutGenerationRequirements.ThisPartyKeyConfirmationRole,
                        _param.IutGenerationRequirements.KeyConfirmationDirection);

                var fixedInfoParameter = new FixedInfoParameter
                {
                    L = _param.L,
                    Encoding = _param.KdfParameter.FixedInputEncoding,
                    FixedInfoPattern = _param.KdfParameter.FixedInfoPattern,
                    Salt = _param.KdfParameter.Salt,
                    Iv = _param.KdfParameter.Iv,
                    Label = _param.KdfParameter.Label,
                    Context = _param.KdfParameter.Context,
                    AlgorithmId = _param.KdfParameter.AlgorithmId,
                    T = _param.KdfParameter.T,
                    EntropyBits = _param.KdfParameter.EntropyBits,
                };

                // KDF fixed info construction

                MacParameters macParam = null;
                IKeyConfirmationFactory kcFactory = null;
                if (_param.ServerGenerationRequirements.ThisPartyKeyConfirmationRole != KeyConfirmationRole.None)
                {
                    macParam = _param.MacParameter;
                    kcFactory = _keyConfirmationFactory;
                }

                _schemeBuilder
                    .WithSchemeParameters(
                        new SchemeParameters(
                            new KasAlgoAttributes(_param.KasScheme),
                            _param.ServerGenerationRequirements.ThisPartyKasRole,
                            _param.ServerGenerationRequirements.KasMode,
                            _param.ServerGenerationRequirements.ThisPartyKeyConfirmationRole,
                            _param.ServerGenerationRequirements.KeyConfirmationDirection,
                            KasAssurance.None,
                            _param.PartyIdServer))
                    .WithThisPartyKeyingMaterial(serverSecretKeyingMaterial)
                    .WithFixedInfo(_fixedInfoFactory, fixedInfoParameter)
                    .WithKdf(_kdfFactory, _param.KdfParameter)
                    .WithKeyConfirmation(kcFactory, macParam);

                var serverKas = _kasBuilder.WithSchemeBuilder(_schemeBuilder).Build();

                var result = serverKas.ComputeResult(iutSecretKeyingMaterial);
                var returnResult = new KasAftDeferredResult()
                {
                    ServerSecretKeyingMaterial = isServerPartyU
                        ? result.SecretKeyingMaterialPartyU
                        : result.SecretKeyingMaterialPartyV,
                    IutSecretKeyingMaterial = isServerPartyV
                        ? result.SecretKeyingMaterialPartyV
                        : result.SecretKeyingMaterialPartyU,
                    KasResult = result
                };

                await Notify(returnResult);
            }
            catch (Exception e)
            {
                await Throw(e);
            }
        }
    }
}
