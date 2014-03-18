using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue102Test
    {
        /// <summary>
        /// Encoded line to simplify
        /// </summary>
        private const string Encoded = @"mfqtGgth\??xAEzAAzAAxAAzAAxAAxACxAIzAGzAGzAE|AEzAC|ACzAAzA?|A@zA@|AB|ADr@Bx@Dv@Bv@Dv@Fx@Ft@Hp@Fr@Hp@Hv@Lv@Jv@Lx@Nx@Nz@Pz@R~@R|@T`ARhAVhAVjAVnAXjAVnAXpAVnAXpAXrAXpAXpAXpAVrAXtAZrAXvAXvAZvAZvAXtAZvAXtAZvAXxAZtAZvAXtAZtAXvAZvAXxAZxAZxAZ|A\~A\bB\hB^fB`@lB^tBb@vBb@|Bb@~Bd@dCb@hCd@nCd@pCd@tCd@xCb@lC^jC\lC\nCXlCTpCNnCJrCFnC@nCA`DIzCKrCSnCWfC[bCa@zBe@tBk@pBm@fBq@tAk@nAi@lAi@hAi@~@g@z@g@t@e@j@c@f@e@^_@f@a@`@]TUPSJODK@IEEIEMASAQ?Q?S?Q@QBQDODOFKJQJMLKRETCTBVDTLZNXT\^b@`@d@d@d@f@h@l@l@p@n@r@p@t@r@z@x@|@x@~@z@bA`AhAdAhAbAhAfAhAdAhAdAnAjAlAhArAnApAlApAlAtAnAvApAxAtAzAtA~AvA~AvAbBzAbB|ArBfBtBjBxBjBzBnB~BnB`CnBfCpBjCpBnClBtChBhC|AnCxApCvAlCpAnCjAjCdAjCbAfCz@fCv@bCp@nCr@jCn@jCh@lCd@lC`@lCXlCVjCPhCJdCFzB@zBCdC?jC@pCDvCF~CJbDJhDNpDNvDPrDNpDNfDL|CJlCLbCLvBLhBN|APnARbANz@Fz@Cz@O|@Y~@e@~@o@|@w@~@cA|@gA`AkA~@gAfAaAjAy@nAm@rAe@vA[vAQzAGxA@tALjAX`A\v@\j@Zd@\ZZXZPXLTPTRR^Ph@Pv@P|@PjAPxAN`BRrBT|BRfCVhCThCThCTjCRfCPjCLjCHdCDdC@dCAbCEdCI~BM`CM|BS|BWzB[|B[|B]|B]zB]~B]zB_@|B]zB_@xB_@tB]nB]lB]jB[fB]dBY`B[`BY~AYxAYvAWtAWtAWtAUrAWrAWrAUrAWpAUpAUnAUnAUnAWnASlAUlAUjAUlAUhAShAUjAShAShASfAQhAOfAQdAMfAOdAMdAKbAK`AK`AK`AI~@G~@G~@G~@Gz@E~@Ez@E~@C`AE~@CbAA`ACbAAbA?dAAdA?jA?jA@nA@tA@tA@|ABbBDbBBjBDlBDrBDzBD|BBbCBfC@lC@nC?tC?xCC|CE`DKdDQhDYlD_@lDg@nDo@nDu@nD{@nDeAlDiAlDqAlDyAfD}A`DaBvCcBpCeBhCeB`CgBxBgBpBgBdBcB|AcBrA_BjA}A`AyAz@sAn@kAl@iAb@cAX}@Tw@Hs@Fk@Be@B_@?WASEKGGICK@MBKFGFCHBLHLNPTT\Vb@\h@`@r@b@~@b@dAb@pA^xA`@`B^dB\nB^rBXxBR`CLfCDlCArCIvCS~C[`De@fDo@jDy@lDaAlDkAjDsAhD{AdDcBdDiB`DmB`DoB~CsBxCsBvCsBpCsBfCmBdCmBxBgBrBaBlB}AbBwA|AsAtAkAlAeAdA}@~@{@v@s@p@m@j@k@d@g@d@g@^c@^e@Za@X]R]T[PWNSLQLOJMJKHIHEHEJEHCJCJ?J?H?JBNBNBRFTF\Fb@Jh@Jp@Lx@J`ANfALnALtALzALbBLhBLpBLvBJ~BLdCHlCHvCF|CDdD?lDGrDMxDW~Da@dEi@dEu@dE}@`EeAxDiAtDqAlDwAfDyAzC}AvC_BnCcBdCcB`CeBzBkBrBkBnBmBlBqBdBuB|AsBxAuBrAsBnAsBhAqBfAkBfAcBfA}AjAyAjAsAlAkApAiArAaArAy@rAu@rAm@rAm@rAk@rAi@lAg@hAa@dA_@`A[|@Y|@Wz@Y~@[`A_@bAc@dAk@fAo@fAw@jA{@lAcAjAgAjAkAjAqAhAqAbAsAbAqA`AoA~@oA`AkA|@eA|@cA~@aA|@}@|@{@|@w@x@s@v@o@v@k@v@i@v@g@z@e@z@e@~@c@bAe@dAc@lAg@pAg@tAg@~Ak@fBo@nBq@vBw@zB{@dCaAfCeAnCmApCoAtCuApC{ApC}AjCeBdCgB`CoB|BoBtBuBnBuBfByBbBwBzA}BtA}BnA}BfA{B~@{Bv@yBl@uBh@uB`@qB\kBVaBVwAPkAL}@Do@Bg@CWKKS?[Hc@Rg@Ti@Xk@Zm@Zm@\i@Ze@Xa@XYTMTCTFRPT\Vd@Xn@\z@`@bAd@fAd@pAf@tAb@~A\bBVhBLpBFpB@rBEtBKpBSrBYpB]nBe@nBk@nBs@nBy@rBaA|BkAbCsAfCyAnC_BpCeBtCgBrCgBpCiBnCiBlCeBhC_BdCwA~BmAzBcArBy@jBm@bBc@zAWtAQlAMdAK|@Mt@Qn@Sh@Wh@]b@a@\e@Zg@Zi@\m@^m@`@k@f@m@l@k@n@k@t@i@z@g@z@g@|@a@z@_@t@[n@Sl@Qh@Kh@Kb@E`@AZ?\BXDXD\H\H\J^Jd@Hf@Hh@Hl@Fn@Dr@Dv@Dv@D|@Bd@Bh@@j@Bn@@p@@r@Bx@@z@B|@B~@BxABxAB|AD~AD~ABfBDhBDlBDpBBtBDvBDzBB|BB~BBdCBfC@fC@hC@jC?hCAnC?lCCjCChCGfCKbCO|BS|BYzB[vBa@rBc@rBg@pBk@nBo@lBu@nBw@lBy@jB}@jB}@lB_AlB}@nB}@nBy@jBw@jBq@fBo@bBi@`Be@~Aa@xA_@vA]pAYnAYhAUbAUz@Qt@Op@Ml@Mf@Mh@Kb@Mb@Mb@M`@M`@Q\O`@QZQXQTSRSNSLSDQDOBMAKEKIIKIMGOGOGOGQEOEOEMCKAI@E@AD?HBNHRHVL\N^Rf@Ph@Vl@Xn@\p@`@t@b@t@f@v@j@x@n@z@p@x@v@|@z@z@`Az@bA|@hA|@nAbAtAfAxAlAbBpAhBxApB~AtBbBxBdB~BfBbCjBfClBlClBnCnBpClBvCnBvClBxClB|CjB~CjB~CdB`DbB~CzA~CtA|CjAxCdAxCz@tCt@pCj@lCd@hC^hCVdCNfCJ~BB~BAxBGtBMpBSnBWfB[dB[|AYxAWpAShAObAKz@Gt@Aj@Ad@BZ?P?J?DA?AICKEKCMCG?C?BFLLTT\\f@d@l@l@t@t@z@~@bAfAhAnAlAvArA|AvA~A~AhBjBnBtBtB|BpBhCtBrCpB|CnBjDjBvDfB`E~AjExArEnAvEdAzEx@~El@bFb@bFTfFJbFA`FM|EYzEe@vEq@jEy@dEeAzDiApDqAfDwA|C}ArCaBhCcB~B_BxB}ArBwAlBsAfBmAbBgAzAcAtA}@rAy@lAq@dAm@bAg@|@c@v@]r@Yn@Uh@Ob@K^I\EZE^C`@Cb@Cd@?h@?h@?l@?l@?p@Ar@Ar@Ar@Ct@Cr@Ct@Et@Ct@Et@Et@Et@Gv@Et@Gt@Ix@Gt@Gv@Ix@It@It@Gx@It@Ev@Et@Cx@Ax@?z@Bx@B|@D|@F|@F|@J`AL~@N`AP`ARdAT`AVdAXfAVhAXlAZnAXrAZvAXvAX~AX~AV`BVfBVhBThBTnBRrBRrBPtBPxBNxBLzBJzBJ|BHzBFzBD|BBzBBzB@vB?vBArBCpBEjBEhBEfBI`BIbBI`BI~AIzAI|AK|AIzAIzAIxAIvAItAIvAIrAIrAGrAGpACrAArA?rA@tA@vADxABxABzAB~AB~ABbBBdBBdBBfB@jBBjBBlBBpB@pB@vB@zBA~BE`CEdCKjCMhCQlCSjCWnC[jC]jC_@hCc@fCe@bCg@`Cg@zBi@xBk@rBk@pBm@hBk@dBm@`Bm@zAo@tAm@pAo@jAo@jAq@dAq@dAq@dAo@bAo@dAo@fAk@hAk@hAe@lAc@pAa@pA]rAYtAStAQtAMvAItAErAApAApA?nA@lA@lA?lAAfACdAEbAG|@K|@Mz@Qv@Sv@Yr@Yn@[p@a@t@c@t@c@x@g@x@g@x@e@v@g@x@g@|@e@bAc@fA[jAWpAQtAKzAE|AAdBDfBHhBNdBV|AVxAVrAZpAXlAZjA\hA\hA\lATnANxAFxA?|AG~AO`BYdBa@fBk@jBs@nBy@vBw@vBs@vBq@xBo@|Bk@dCk@nCk@tCk@vCo@xCo@rCo@dCk@xBe@hB]zAUlAK~@Gn@A\BVBRBV?Z?\Cd@Ij@Sp@[r@e@n@i@p@o@l@q@p@i@t@a@z@Wx@Qx@Gr@@j@Ff@R`@V\ZTXPXJT@JGBQCYM[O]WUWIW?YFWNQTK\Ab@Fh@Pn@Vn@^n@b@j@j@h@n@b@p@Zp@Vt@Vt@Zr@^r@f@n@l@j@p@l@v@l@x@r@bAv@jA~@tAdA|AlAdBpAlBvAtBzA|B`BdCfBnChBtCjB~ClB`DhB`DbBzCxAxCrAtCjApCbAtC~@rCv@rCr@pCl@pCf@nC`@lCZlCVjCPjCJdCFfCBdC?bCA~BC|BCvBEtBCnBClBChBCfBA`B?~A?zABvABpADnAFhAHfAH`AJ|@Lx@Lz@Lt@Lt@Lr@Lt@Nv@Lt@Lv@Lv@Lt@Jv@Jz@Lz@L|@J|@L~@L`ANbAN`ANdARdAPfATdAVdAVfAXdAZfAZdA\bA\bA\dA`@dA^dA^dA^bA`@dA^fA^dA`@hA^hA`@hA`@jA`@lAb@nAb@pAd@pAd@vAd@vAf@xAh@|Ah@`Bj@`Bl@bBj@dBl@fBl@hBn@lBn@lBp@pBn@pBp@rBp@tBp@vBr@xBn@xBl@zBn@~Bl@|Bh@bCh@dCd@dCd@hC`@fC\jC\lCXlCVnCRlCPnCNlCHnCHjCBlCBjC?hCCfCEdCI~BI~BOvBOvBQrBSnBSjBUfBUbBU~AU|ASvAUtASrASnAQlAQhAOhAMbAMbAI~@I|@Gz@Ex@Ex@Av@Av@Ar@?t@@t@@r@Bt@Dt@Dt@Fv@Ht@Hv@Jx@Jz@Lz@L|@L~@N~@N`ALdAPbANdANfANfANhANjAPjANnAPnANnAPrAPpARrAPtAPtARtAPvARtAPxARtARvAPvARrARvARtAPtARtARtARtARtARtARtATtARtARtATvARtATtARtATtARvATrARvATvATtARtATrARtARrATpARpAPnARnARjAPlARjAPjAPhAPjAPhANhAPjAPjARnAPhARjARjARjATfATfATdAVdAV`AV|@V|@V|@Vz@Vx@Vv@Vv@Vt@Tv@Tt@Tt@Tr@Tt@Tp@Rr@Tr@Tt@Xr@Vt@Xt@Zv@\v@^x@`@t@`@x@b@v@b@v@d@v@f@v@f@v@h@t@h@v@j@t@j@v@l@v@n@v@n@x@p@x@p@x@r@z@p@x@r@z@p@z@r@~@v@~@t@~@t@~@t@`Av@|@t@`Av@|@t@|@r@z@p@|@r@v@n@v@l@v@l@r@h@r@h@p@f@p@f@n@b@l@`@j@^f@\h@Zb@Tb@V`@T\T\RVPVPRNNLLLLLJLHHFHDFDHDHBHBJBJDPDRDTF\H`@Jh@Jp@Jz@N`ALdAPlAPrAR|AT`BThBXlBZrB\xB\`Cb@dCd@jCf@rCj@tCp@`Dt@bDx@jD|@lDbArDfAxDlAzDnA|DtA`ExA~DzAzD|AzDbBzDbBvDhBxDjBvDnBtDpBpDtBpDvBjD~BpD|BhD`CdDbC`D`CzCdCvCdCpCdCjCdCfCbC`C|BtB|BpB|BlBzBjB|BdBvB`BxB~AxBxAtBvAtBrA|BtAxBrAxBnAvBlAtBhArBfArBdAnBbAnB`AjB|@fBz@bBv@bBv@`Bt@bBt@~At@|Ar@|Ap@|Ar@|Ar@~Ar@~At@`Br@`Bt@`Bt@bBt@dBv@fBv@fBv@jBx@jBx@lBx@nBx@pBz@pBz@tB|@vBz@zBz@zBz@~Bz@`Cx@bCv@dCv@fCr@hCn@hCl@jCj@jCf@jCb@jC^jC\hCXhCTfCPbCNbCH|BH|BBzB@tB?tBCnBEjBEfBIbBI|AIxAKtAKpAKlAKhAKfAI`AGbAE~@C~@A|@?|@@|@B|@D~@H~@J~@LbAN~@R`ATbAV`AX`AZ`A^bA\bA^dA`@fA^hA`@lA^pA^pA\vA\vAZxAVzAT~ATbBRdBPhBRlBNrBPrBPtBP`CR`CPbCPdCPdCPdCNdCNbCLdCJ`CJ|BF|BFxBFzBBvBBvB@xB?vBAvBCvBCzBEzBGxBIzBIxBKxBMzBQxBOvBSvBStBSpBUrBWtBYrBYrBYpB[tB[tB]tB]vB]xB_@vB_@xB_@xBa@zB_@xBa@zBc@xBa@|Bc@xBe@zBe@xBg@zBi@xBk@vBk@vBm@vBq@vBs@rBs@rBu@rBw@lBw@jBy@hBw@dB{@bBy@~A{@|A{@xA{@rAw@nAw@jAw@fAu@bAs@`As@z@o@x@m@t@m@p@g@l@g@l@c@f@a@f@[b@[b@U^U`@QZM^M\KZGZGXEZCZA\?Z?\@\Bb@Db@Df@Fh@Hj@Fl@Hn@Hp@Ft@Ft@Fz@Fz@D|@F~@D`ADdADfAFdADhADjADdADfADfADfADfADhADfADfADdADdADhADfADfAFfAFfAFbAFfAHdAHbAHdAJdAJdAJdANfALhAPhAPhARlARlATnAVnATnAVpAXrAZtAXvAZxA\xA\|A\zA^bB^bB^fB`@hB^jB^lBZpB\pBZtBXtBXrBTvBTvBRxBPxBPxBLzBLxBJzBHxBFvBFxBBvBBtB?tB?rB?pBCnBCnBEjBGnBGnBGlBIjBIjBKfBIhBIdBIdBIbBG`BI~AG|AEzAGzAEzAGvAEtAEvACtAEnAClACnAClAAlAClAAlACnAAnAAnAAtAAtA?vAAxA?xA?xA?zA?zA?|A?~A@zA?~A@|A?~A?bB@`B?`BAdB?dB?dBAjBAjBCjBEnBClBGrBGpBGrBIrBKtBMtBMtBMvBOvBQxBSxBSzBUzBWxBW|B[~B[`C]`C_@~Ba@`Cc@|Bc@~Be@|Bg@|Bi@zBi@tBi@tBi@rBk@pBm@nBm@nBo@jBo@jBq@hBq@fBq@fBs@dBu@dBs@~As@bBu@|As@|As@zAs@|As@xAq@xAq@vAq@vAo@tAm@rAm@rAk@pAk@rAi@nAi@nAg@pAe@nAg@lAc@nAe@lAc@lAa@jAc@nAa@jAa@lAc@jAa@lAa@jAa@jAc@jAa@jAa@hA_@jAa@hAa@hAa@hAa@hA_@fAa@hAa@fA_@fA_@fAa@fA_@dA_@bA_@dA_@dA]bA]bA[bAY`AY`AY`AW~@W~@U~@U~@S|@S|@Qz@Q|@Oz@Oz@Ox@Mv@Mv@Kv@Kx@Kr@Kt@Gt@Ip@Ip@Gp@En@Gj@El@Eh@Eh@Ch@Ef@Ch@Ed@Cf@Cf@Cj@Aj@Al@Aj@?n@Bl@@l@Dn@Dn@Fn@Hl@Hn@Ln@Lp@Nn@Pn@Pp@Rp@Tp@Rr@Tr@Tr@Tt@Tv@Vx@Vz@Vx@V~@X|@X`AX~@Z~@XbAZ`AZbAZbA\bA\bA\`AZbA\bA\`A\`A\`A\|@Z~@Z|@\z@Xz@Zz@Zx@Zv@Xr@Xt@Xr@Xr@Zn@Zp@Zn@Zl@Zp@^l@\l@`@j@\j@^j@^f@\f@^f@^d@^\X`@ZZZ\X\Z\Z\\Z\\\\^b@d@b@f@b@f@d@j@b@j@d@l@f@n@d@p@f@t@f@t@j@z@h@z@j@~@j@bAl@dAn@hAn@jAp@pAp@pAt@vAp@rAt@xAt@xAx@~Az@bB|@bB~@hBbAlBdAlBfApBhApBlAtBnAxBpAxBtAzBxA|B|A|B~A`CbB~BfB`CjBdCnBdCrBbCtBbCxBbCzB~B|B~B`C|BbC|BdCxBfCtBfCrBjCpBjClBnCjBlCfBpCdBrCbBrC|AtCzAxCxAzCtAxCpAzClAxCjAzCdAvC~@vC|@vCv@rCt@nCn@pCj@lCh@lCb@jC`@hC\fCZfCVfCTbCPfCNdCLbCJ`CH~BH~BDzBFzBDvBDvBBjBDhBBfBBdBB`BB`BB~AB~A@zABxA@|ABxABxA@tABrA@pA@nABhA@lA@fA@fA@bA@bA@~@?|@?|@@x@Av@?v@Ar@Av@?r@?r@?r@@n@@n@Bl@Bl@Dj@Fl@Hp@Hn@Ll@Lp@Pn@Pp@Rp@Vp@Vn@Xt@\f@Vf@Zj@Zj@\l@^n@`@r@d@t@d@v@d@v@f@bAn@fAn@lAr@lAp@nAt@rAr@xAv@xAv@zAv@~Av@xAr@zAr@|Ar@~At@`Br@`Br@dBr@dBr@fBp@dBr@rBt@rBt@rBr@rBr@rBp@pBn@pBl@nBj@nBj@jBf@~Ab@~A`@zA^xA\tAZtAXpAXnAVlAVjARlATjAThAPbAPbAP~@N~@Nx@Pv@Lt@Pr@Nl@Nl@Pj@Pf@Rf@Pd@Rd@Rb@Tb@TZR\RZT^V\X\X^\`@^d@b@b@d@h@j@j@n@l@p@n@t@p@x@r@z@t@~@v@`Az@bAz@dAdAlAdAlAfAnAlArAlArArAtArAvAvAvAzAvA|AzA|AtA~AvA`BxAdBvAfBxAjBxAlBxAlBvAnBvApBtAxBxAxBvAxBtA|BtA|BrA|BpA|BnA~BjA~BjA~BhA|BdAzB`A|B`A|B~@zBz@|Bz@~Bx@zBv@~Bt@|Bt@bCr@~Bp@bCn@`Cl@`Cl@`Ch@`Ch@bCf@`Cb@bCb@~B`@`C^`C^`CZ`CZ`CX`CV`CT`CR`CRbCNdCNbCNbCJbCHbCF~BF`CB~BB~BBvB?tB?tBAtBCrBCpBEpBGpBGnBIlBKrBKpBKpBMlBOnBMlBOhBMjBOhBOhBObBM`BO~AM~AM~AM~AMzAOzAMzAMxAK|AOzAMzAMzAMxAMzAMxAMzAMxAKxAMvAMvAMxAKvAMxAMxAMzAMzAM|AM|AM`BOdBMbBOdBOdBOfBOdBOfBOhBOfBObBOfBQbBQdBOdBQbBSbBSbBS`BW`BUbBY~AY~AY|A[|A[|A[zA]zA[zA]vA[xA]tA[tA[tAYrA[pAWnAYlAWlAWjAShAUfASdAQdAQbAO`AM`AM~@M`AK~@K~@K`AI|@I`AI~@G~@E`AG`AE`AEbAEdACfACfACjAAhAAjAAjA?nA?nA?nA?lA?lA?nA@pA?pA?rA@tA?rA?vA?vA?zA?|A?|AA~AA~AA`BAbBA`BCdBCbBCbBCdBCbBEbBEdBEbBG`BEdBGbBGbBIbBG`BI~AIbBI`BI~AKdBK`BKbBKbBKfBMfBMhBMjBMjBMnBOnBOpBMpBOrBQdBMfBOhBOhBOjBQjBQlBSlBUlBUjBW|B]zB_@zBa@|Bc@zBe@zBi@vBi@vBm@vBm@pBo@hBm@hBq@`Bo@|Ao@xAo@tAo@pAo@jAm@hAm@bAk@`Ak@|@i@z@i@x@g@v@g@t@g@r@e@p@c@p@a@n@]l@[l@Uj@Sj@Mj@Kf@Gf@Cf@?b@Bb@F^F\JVJVJPHPHJDJDJBH@H?JAJCHELELIJINKPIPIRGTEXCT?V@VBTDTFTHRLRLRNPPPTNTLVPXNZP\P^Tb@Xb@Zd@`@f@d@j@f@j@j@p@n@r@r@t@t@x@z@|@|@~@`AbAdAdAfAjAjAjAnAnApArArArArArAvAtAtArAvAtAvArAvApAtApAxAnAxApAxAlAxAlAzAlAzAjAxAfAzAhA|AhAzAfA|AdA|AfAzAdA|AdA|AdAzAbA|AdAzA`AzAdAzA`AxA`AvA`AxA~@tA~@rAz@pAz@pAz@nAz@lAx@pAx@lAx@lAv@jAv@lAx@pAz@pAz@pAz@nAx@pAz@pAz@pAz@pAz@nAz@rAz@lAx@nAx@nAx@nAx@nAx@nAx@nAz@nAx@nAz@nAx@pAz@pAz@rA|@vA~@xA`A|AbA~AdAbBfAdBhAjBlAlBlAnBnApBnApBnArBnArBnArBnAtBlArBnAtBjArBjAtBjAvBdAxBdAzB~@|B|@`Cv@dCv@jCp@nCn@rCl@vCh@zCf@~Cd@bDb@fDd@hD`@nD`@nD\tD\rDZtDVpDRnDPjDLhDHdDDbDA|CE|CIxCQxCSxC[xCa@xCe@xCk@|Cq@zCw@~C}@`DcA`DeA`DkAbDmAbDoAbDqA`DsA`DuA|CwAxCuAvCwApCyAnCyAhCyAdCyA~ByAxBwAtByAnByAjBwAbBuA`BuAxAsAxAuArAsAlAoAlAqAfAoAbAmA`AkA|@gAv@eAt@aAn@}@j@y@d@s@`@o@\k@Xe@Ra@P]LWLWHSLQLOPQRMXK\M`@Kd@Il@Ir@Gv@G~@EdAAhAApA?tA@|A?dB?hBArBGvBK~BQdCYjCa@nCi@vCs@zC}@~CeAbDqAfDyAhDeBjDoBlD{BlDcClDmCjDuCdD}CbDeD|CkDvCmDnCoDhCqD`CqDxBqDrBmDjBiDdBgD`BeDxA_DtA{CnAuCjAqCfAkCbAeC~@}Bz@wBx@qBv@iBp@aBn@yAn@uAj@oAh@iAd@cAd@_Ab@y@`@u@^q@^o@\k@Zg@Zg@Zc@Xc@Xa@Za@Za@Za@\a@^c@^c@^a@b@c@b@e@d@e@f@e@h@e@j@e@j@g@l@e@n@e@n@e@p@g@t@e@t@g@t@e@x@g@x@i@x@g@z@i@z@k@|@k@~@m@~@m@`Ao@bAq@bAq@dAq@dAs@fAs@hAu@hAu@jAw@jAw@lAw@lAy@lAy@nAy@lAy@pAy@nAw@nAy@lAy@nAy@nAy@nAy@nA{@nAy@nAy@lAy@nA{@nAy@jAu@fAu@dAq@`Aq@~@o@`Ao@~@m@`Ao@~@q@`Ao@~@o@`Ao@~@o@~@o@~@o@~@o@~@q@~@o@~@o@~@q@`Ak@`Ai@bAc@bAa@dA_@dA_@bA_@dA_@dA_@dA_@bA_@dAa@fAa@lAc@lAe@pAg@pAe@nAg@pAe@pAg@tAg@vAk@zAk@~Ao@`Bm@`Bo@`Bo@`Bq@hBq@fBu@nBu@nBy@pB_AtBcAvBmAxBoAxBqAxBqAxBqAvBqAxBuAxBuAvBsAvBsAxBuAtBuArBuAtBwArB}ApB_BpBeBjBeBhBiBbBiB|AkB`BoB~AoB`BsB~AqB`BoB~AsB|AqB`BqB|AsB~AqBzAoBxAkBxAmBtAgBrAgBnAaBlA_BhA{AfAwAdAuA~@mA|@kA|@mA|@kA|@mA|@kA|@kA|@kA|@kA~@mA`AqAbAsAbAqA`AsAbAsAdAuAdAuAhA{AjA}AnAcBpAeBrAgBvAkBxAoBzAsB~AwB~A{BbB_CdBcCdBeChBkCjBqCjBsCnB{CpB_DnBcDlBgDjBmDfBoDdBuD`ByD|A}DzAaEvAeEtAgEnAkEjAmEhAqEdAqE~@sE|@uEv@yEr@uEp@{Eh@{Eh@yEd@uEb@qE`@oE^gE\cEZ_EZ{DXuDXoDTmDVgDTeDR{CRwCRsCPmCPiCNcCP}BN{BN{BNyBN{BN{BLyBN{BN{BNwBNuBLqBLmBLeBJaBJ{AHwAHqAHmADgAFcAD_ABy@@u@@o@Ak@Ai@Ag@Cg@Cg@Ai@Ag@@e@@e@De@Dc@Hc@H_@L_@L_@N[R[R]TYXYXWZYZYZ[Z]Z_@\c@^i@\g@^k@^k@\k@^m@`@o@^q@^q@`@u@`@u@b@{@b@_Ad@aAd@cAb@_Ab@cA`@_A`@}@^_A^{@^y@\s@^o@\i@Ze@Za@V[VWTSPQPONMLMLKLMHMLKJOJMLMPQTQVQ\Q`@Qd@Qh@Oj@Mn@Mn@Ip@Kp@Ir@It@Kt@Mt@Kv@Ox@Mx@Oz@Q|@S|@SbAUdAWfAYjA[nA]pA]rA]vAa@vA_@zAa@|Aa@~Ac@`Bc@bBc@dBe@fBe@fBe@dBe@fBe@`Be@~Ac@zAa@xA_@rA_@pA]jA[fAYfAY`AWbAW|@[|@]z@_@|@a@z@c@z@e@z@g@x@e@z@e@|@c@z@a@|@]|@Y~@U|@Q~@M`AK`AE~@C~@A|@?z@?x@@t@?p@@j@@f@@f@?`@@^?^A^A^C^C^C`@E`@Gb@Gb@Gf@If@If@Kh@Id@Ih@Gp@Cr@?x@D~@HdALhATnAXtA\xAd@|Af@bBj@dBp@hBr@nBr@pBv@nBt@pBr@rBt@rBt@vBr@xBl@`Cf@dC^hCZnCRtCJvCDzCCvCIrCOrCWrC_@pCc@pCi@rCm@nCq@nCo@nCs@lCs@lCq@dCq@bCq@zBq@xBo@nBm@lBo@dBk@bBi@zA_@xAWtASnAMhAMfAKdAKbAIbAKdAKbAKdAKbAK`AK|@Iv@Ip@Ij@Gd@Gd@Gh@Ip@Kr@Mt@Kv@Mt@Mt@Mt@Mv@Ot@Ov@Ot@Mv@Ot@Ix@Gt@?v@Bt@Hx@Nt@Nv@Pv@Pv@Nt@Nx@Pv@Nv@Nv@Nv@Lv@N~@NbANhAPtARzARfBTrBX|BXbCXfCXjCXfCXjCVhCVjCTjCTjCThCRjCRjCPjCHjCBjCCjCIjCOhCWjC]fCe@hCg@hCk@fCm@fCm@fCm@bCo@dCo@fCo@dCq@bCo@bCq@bCq@|Bo@xBo@rBm@lBi@jBi@bBg@|Ae@xAc@tAc@tAc@tAa@rAa@tAa@tAa@pA_@rA_@tA_@rA_@tAa@pAa@tAa@tA_@tAa@rAa@tA_@tAa@vAa@xAc@|Ac@bBg@fBg@dBi@fBi@fBi@fBe@hBg@hBg@fBg@fBi@fBi@fBg@fBi@hBi@dBi@hBi@fBi@fBi@dBo@dBs@dB{@`B}@`B}@~A}@`B}@dB_AbB_AbB_AbB}@`B_AbB}@bB_A`B_AbB_A`B_A`B_A`B_A`B}@|A}@xAy@tAw@rAu@pAu@pAu@nAs@fAo@fAm@bAk@z@g@v@e@r@a@n@_@j@_@j@_@j@]j@a@j@_@j@a@h@a@h@a@h@c@h@e@h@e@h@c@j@a@j@[l@Yn@Un@On@Ir@En@?r@Bp@@p@?p@@r@?n@@r@Ap@?n@Ap@An@Ar@Ar@Cz@E|@E~@GhAGjAGrAIvAIxAIzAKvAIzAIxAKxAIzAIxAIzAKvAIzAIxAKxAIxAKxAIxAIxAIxAKxAIxAIxAKxAIxAIxAIvAKzAIvAItAInAIjAGdAI~@Gz@E|@Gz@Iz@E|@Iz@Iz@I|@Iz@Iz@I|@Iz@Kz@Iz@Kz@Iz@G|@Az@B|@Hz@LbARfARlATtARvAT~ATbBVhBTnBVrBVxBV~BVdCVhCTnCVtCRdDRnDPvDJ~DHfEChEKfEUfEa@`Eg@|Dq@vDy@nD_AhDeA`DkAvCoAnCuAhCyA~B{AvB_BlB_BhBeBfBkB~AkB~AkBzAoBtAiBrAiBpAcBnA}AnAyAnAqAnAkAlAaAjA{@jAq@fAk@jAe@hA]jAWlAUjAOlAOnAOpAQrAUzAY~A]dBe@lBm@tBu@~B}@fCgApCsAvC{A`DgBhDqBlDcCpDoCvD_DrDmDvD{DnDgEjDoEbDuE|C}ErCaFjCeFbCiFxBkFrBmFfBmF`BmFvAoFnAiFhAmFbAgFz@cFt@}Ep@wEl@mEh@iEf@aEd@wDb@oD`@eD`@}C\uC\kC\cCZ{BXqBVgBVaBV{AVsAToARiATgATcATeAVeAVeAXgAXeAZkAZiA^mA\mA^mA^mA`@oA`@qA^oA`@qA^mA`@oA\mA^kA\gAZgAZcAXcAXcAV}@T{@T{@Ry@Pu@Pw@Ns@Lq@Nq@Jo@No@Lm@Nk@Pm@Rm@Rk@Xk@Xk@\m@^k@b@m@d@m@h@m@l@m@n@m@n@k@r@m@t@m@t@m@x@o@x@o@x@o@z@s@z@q@z@u@z@w@z@y@|@}@x@}@x@_Ax@aAv@cAr@cAr@gAp@gAp@eAl@gAl@gAl@gAj@gAh@eAj@eAh@eAl@cAh@aAl@aAl@_Al@{@p@{@n@w@t@u@t@s@v@o@z@o@z@k@|@i@`Ag@bAe@bAe@dAe@hAe@hAe@lAg@nAk@pAk@rAq@vAs@xAy@|A}@~AeAbBmAfBqAjB{AjB_BnBgBpBmBrBuBvB{BvB_CxBgCvBkCxBoCzBuCvBuCvB{CtB}CtBcDrBcDpBeDnBgDlBiDjBmDhBmDbBmDbBmD~AmDzAoDxAmDtAiDrAiDlAeDhAaDfA_DbAyC~@uCx@qCv@kCr@gCl@aCj@{Bd@yBb@sB\oB\iBVcBV_BR}ARyANoANkANiAL_AL_ALw@Lq@Lm@Lg@Ja@J]J[JWJQJOJMJKJIJEJENENGPERITIVKZMZO^Q`@Ub@U`@Yf@Yd@]f@]j@a@h@a@j@c@l@e@n@g@n@i@n@g@n@i@n@i@p@i@l@i@n@g@l@g@l@g@j@c@f@c@f@a@b@]b@]^Y^YZWXUVSVSVQXQXOXO\O`@Od@Of@Ml@Mp@Kx@Kz@I`AIfAEjAEnACtAAzAA~AAdBAjBClBCpBGxBItBM|BQ|BU`C[`Ca@bCe@dCk@fCq@jCy@jC}@nCeAnCkAnCoArCyArC}ArCeBrCkBrCoBnCsBnCuBjCwBlCyBfCyBfCwB`CwB`CuB|BwBxBqBvBsBrBqBpBoBlBmBjBkBdBkBbBgB`BgBzAeBzAcBtAaBtAcBtAcBpAcBnAcBpAgBnAcBlAgBlAeBlAgBjAgBjAeBhAeBfAeBhAcBdAaBfAaBdAaB`A}AbA{A`A{A~@wA~@wAz@sAz@sAz@oAz@oAv@kAv@kAx@iAt@gAv@gAt@eAt@eAt@cAr@aAt@aAt@aAt@aAr@}@t@_Ar@}@t@}@t@}@r@{@r@{@r@y@t@y@r@w@r@y@r@u@p@u@r@u@p@u@p@s@p@q@n@q@p@q@p@o@n@o@l@m@n@m@l@k@l@k@j@i@l@k@j@i@h@e@j@g@h@e@j@e@h@c@h@a@h@a@h@a@h@_@h@_@h@]h@[j@[h@[j@Yj@Yl@Yl@Wl@Wn@Un@Up@Ur@Ur@Ur@Sv@Sv@Sx@Ux@Sz@S~@S|@S~@U`AS`AS`ASbAU`AS`AU`AS~@U`AS~@U~@S~@U|@S~@U~@U|@U~@S|@W~@S|@U~@U~@S~@S~@S`AO~@O`AM`AMbAIdAIdAGdAEfAEhAChACjAAjAAnAAnAApA?pAAvA?vAAvAA|AA~AA`BCdBEhBEhBGlBIpBIpBKvBOvBOxBSzBS|BW~BW~B[`C]bC_@bCa@bCc@dCe@dCi@dCi@bCk@bCo@bCo@~Bo@|Bs@zBq@vBu@rBs@nBs@lBs@fBs@bBq@|Aq@vAq@rAo@nAm@fAm@dAm@~@k@x@k@t@i@n@g@j@e@d@a@b@a@^a@\[Z[ZYZWXSXQVOVIVITCTCR?TBRBTHTHRLVNXPXR^R`@Rd@Rh@Pj@Pp@Pv@Pz@R|@P`APfARjARlAPrARvARxAR~APbBRdBPhBPfBPhBNjBLjBLjBJnBHlBHpBFpBHrBFrBFtBFtBDtBDtBDtBDtBDrBBtBDpBBpBBrBDpBBpB@rB?rB?rBAtBCxBExBG|BI|BK~BM`CObCObCQhCShCSjCUlCUnCWpC[rC]rC_@rCc@rCe@pCi@nCk@nCo@jCq@jCu@hCw@dC{@fC{@bCaAbCcAbCeA`CiA~BkA~BoA~BqAzBsAxBuAvBwArBuArBwAlBuAjBuAhBsAfBsAbBoA`BmAzAkAzAkAtAgAtAeApAcAnAaAjA}@hA}@fA{@bAy@bAw@`Aw@`Au@~@u@~@s@~@s@~@q@|@q@`Ao@`Ao@`Ao@`Am@bAk@dAm@dAi@fAm@fAk@jAk@jAk@lAm@nAm@rAo@rAo@tAq@xAs@zAu@zAu@~Aw@`By@bB{@bB{@fB_AdBaAfBaAfBeAhBeAhBiAhBiAhBoAhBoAfBqAhBuAdBwAfByAfB}AbB}AdBaBbB_BbBcB`BcB~AcB~AaB|AcBzAaBzAaBxAaBxAaBvA}AtA_BrA{ArA{ArA{AnAwApAyAjAuAlAsAjAsAjAsAfAoAhAqAfAmAdAoAdAkAdAmAdAmAbAkAbAkAdAmAbAkAdAmAdAkAdAmAfAmAdAoAfAmAdAmAdAmAfAmAdAmAdAmAdAkAbAkAbAiA`AiA`AgA~@gA|@eA~@cA|@cAz@cAz@aAx@aAz@aAv@_Ax@_Ax@_Ax@_Av@_Av@}@z@_Ax@_A|@_A|@aA~@_A`AaAbA_AdAaAfAaAhAcAlAaAnAcAlAcArAeArAeAvAgAxAkAzAkA|AoA`BsAbBsAdByAhB}AhBcBlBgBpBmBnBqBrBwBrB{BtBcCtBiCtBmCrBqCtByCpB}CpBcDnBeDjBgDhBmDjBuDlBcEhBgEhBkEbBmE~AoEzAkElAyDfAqD|@cD|@cDv@}Ct@}Cr@yCt@}Ct@kDt@iDr@eDr@cDn@aDj@_Db@aD^aD^_DZ{C\yC`@wCh@qCl@kCt@cCt@eCt@aCr@_Ct@aCn@_Cf@eCd@eCd@eCb@cCd@cC`@_C^oBZoB\oBXkBXkBZoBZ}B\cC^mCZiC\gCZcCV}BVqBN}ANwALsAJoAJkAHeAFcAD}@Bw@Bs@@k@@e@@c@?]?U?QAIAGACC?C?CBCBCBCDABAFABAF@B@@DBD?H?LCLCTEVI\Kd@Qh@Wp@[t@a@|@i@bAm@hAs@nAy@vA_AzAeAbBkAhBoAjBuAnBuAnB{AtB}AvBaBtBeBvBgBpBeBjBgBdBgB|AcBtAcBlA_BfA_B|@{Av@wAn@wAh@wAf@yAb@yA`@yA^wA\wA^yA^wA^wA`@wA`@mAb@gAd@}@`@s@b@k@`@_@^WZIV?RFLPDTBVCZG\K\OXSXWRYPYHSDW?QAOGMKMKGMAQAMGMIMKKKKMKKIQEQEUEYC[C]C]A[C_@A[AWAU?SAKB@JFNJTPXV\\b@d@l@j@p@r@x@x@`A|@dAfAjAjAnArAtAzAxA`B|AfB`BnBbBtBdB~BhBfCjBnChBvCjB`DjBjDfBpDbB|DbBdEzAnEvAxElA|EdA`Fz@hFn@fFd@jFZhFNjFBhFEdFSdF_@~Ei@zEs@vE{@pEeAlEoAbEsA~D}AvDcBpDgBhDmBbDoBzCsBvCuBnCsBlCsBdCqB`CoB|BmBtBgBpBgBjBaBfB_BbB{A|AuAvAsAtAmAnAkAjAeAfAaA`A}@|@w@z@u@v@s@v@o@t@o@r@m@r@m@p@i@n@g@p@g@n@g@l@c@n@e@l@c@j@_@j@a@j@_@l@_@h@[j@]f@Yd@Wf@Wf@Ud@Uf@Uf@Sh@Sh@Sh@Sn@Sn@Qn@Sn@Qp@Qp@Or@Qp@Mt@Qt@MfAShAShAQdBYhBWfBWfBUhBWfBS~AQvAOvAOxAOtAOxAMvAMvAMvAMvAMvAMvAMvAKvAM@?";

        // Number of finished jobs
        private static int _finishedJob;

        /// <summary>
        /// Process :
        /// 1) Removes files from directory
        /// 2) Run n simplify function in parallel and writes each result
        /// </summary>
        [Test, Category("Issue102")]
        public void TestRun()
        {
            const double Tolerance = 0.0005;
            const int Max = 100;

            _finishedJob = 0;

            try
            {
                LineString line = Helper.GetLine(Encoded);
                ILineString res = (ILineString)TopologyPreservingSimplifier.Simplify(line, Tolerance);
                Simplify(line, Tolerance, res, 0);

                for (int i = 1; i <= Max; i++)
                {
                    int index = i;
                    WaitCallback callback = delegate { Simplify(line, Tolerance, res, index); };
                    ThreadPool.QueueUserWorkItem(callback);
                }

                do { Thread.Sleep(50); }
                while (_finishedJob < Max);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Simplifies the line with the given tolerance.
        /// Writes the result in file.
        /// </summary>
        /// <param name="line">Line to simplify</param>
        /// <param name="tolerance">Tolerance to use by simplify function</param>
        /// <param name="supposedResult">The supposed result</param>
        /// <param name="index"></param>
        public void Simplify(ILineString line, double tolerance, ILineString supposedResult, int index)
        {
            try
            {
                Console.WriteLine("Job {0} started", index);
                IGeometry geometry = TopologyPreservingSimplifier.Simplify((ILineString)line.Clone(), tolerance);
                Assert.IsTrue(geometry.Equals(supposedResult));
                Console.WriteLine("Job {0} terminated", index);
            }
            finally
            {
                Interlocked.Increment(ref _finishedJob);
            }
        }
    }

    public class Helper
    {
        private const int MinASCII = 63;
        private const int BinaryChunkSize = 5;

        /// <summary>
        /// Décode une chaîne de caractères encodés et créé la ligne associée
        /// </summary>
        /// <param name="encoded">Encoded polyline</param>
        /// <returns></returns>
        public static LineString GetLine(string encoded)
        {
            List<Coordinate> locs = new List<Coordinate>();

            int index = 0;
            int lat = 0;
            int lng = 0;

            int len = encoded.Length;
            while (index < len)
            {
                lat += DecodePoint(encoded, index, out index);
                lng += DecodePoint(encoded, index, out index);
                locs.Add(new Coordinate(lng * 1e-5, lat * 1e-5));
            }

            return new LineString(locs.ToArray<Coordinate>());
        }

        private static int DecodePoint(string encoded, int startindex, out int finishindex)
        {
            int b;
            int shift = 0;
            int result = 0;
            do
            {
                //get binary encoding
                b = Convert.ToInt32(encoded[startindex++]) - MinASCII;
                //binary shift
                result |= (b & 0x1f) << shift;
                //move to next chunk
                shift += BinaryChunkSize;
            } while (b >= 0x20); //see if another binary value
            //if negivite flip
            int dlat = (((result & 1) > 0) ? ~(result >> 1) : (result >> 1));
            //set output index
            finishindex = startindex;
            return dlat;
        }
    }
}