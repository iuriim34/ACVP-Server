﻿using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.DSA.ECC;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Builders;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Enums;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.KDA;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Scheme;
using NIST.CVP.ACVTS.Libraries.Crypto.KAS.Scheme.Ecc;
using NIST.CVP.ACVTS.Libraries.Math;

namespace NIST.CVP.ACVTS.Libraries.Crypto.KAS.Builders.Ecc
{
    public class KasBuilderKdfNoKcEcc :
        KasBuilderKdfNoKc<
            KasDsaAlgoAttributesEcc,
            OtherPartySharedInformation<
                EccDomainParameters,
                EccKeyPair
            >,
            EccDomainParameters,
            EccKeyPair
        >
    {
        public KasBuilderKdfNoKcEcc(
            ISchemeBuilder<
                KasDsaAlgoAttributesEcc,
                OtherPartySharedInformation<
                    EccDomainParameters,
                    EccKeyPair
                >,
                EccDomainParameters,
                EccKeyPair
            > schemeBuilder,
            KasDsaAlgoAttributesEcc kasDsaAlgoAttributes,
            KeyAgreementRole keyAgreementRole,
            KasAssurance assurances,
            BitString partyId)
            : base(
                  schemeBuilder,
                  kasDsaAlgoAttributes,
                  keyAgreementRole,
                  assurances,
                  partyId
              )
        {
        }

        public override IKas<
            KasDsaAlgoAttributesEcc,
            OtherPartySharedInformation<
                EccDomainParameters,
                EccKeyPair
            >,
            EccDomainParameters,
            EccKeyPair
        > Build()
        {
            var schemeParameters = new SchemeParametersEcc(
                _kasDsaAlgoAttributes,
                _keyAgreementRole,
                KasMode.KdfNoKc,
                KeyConfirmationRole.None,
                KeyConfirmationDirection.None,
                _assurances,
                _partyId
            );

            var kdfParameters = new KdfParameters(_keyLength, _otherInfoPattern);
            var scheme = _schemeBuilder.BuildScheme(schemeParameters, kdfParameters, _macParameters);

            return new Kas<
                KasDsaAlgoAttributesEcc,
                OtherPartySharedInformation<
                    EccDomainParameters,
                    EccKeyPair
                >,
                EccDomainParameters,
                EccKeyPair
            >(scheme);
        }
    }
}
