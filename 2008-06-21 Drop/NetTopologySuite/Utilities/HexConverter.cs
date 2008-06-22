using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    public static class HexConverter
    {
        /// <summary>
        /// Convert the given numeric value (passed as <see cref="String"/>) 
        /// of the base specified by <paramref name="baseIn"/> to the value specified by 
        /// <paramref name="baseOut"/>.
        /// </summary>
        /// <param name="value">Numeric value to be converted, as <see cref="String"/>.</param>
        /// <param name="baseIn">Base of input value.</param>
        /// <param name="baseOut">Base to use for conversion.</param>
        /// <returns>Converted value, as String.</returns>
        public static String ConvertAnyToAny(String value, Int32 baseIn, Int32 baseOut)
        {
            String result = "Error";

            value = value.ToUpper();
            const String codice = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // test per limite errato sulle basi in input e/o in output
            if ((baseIn < 2) || (baseIn > 36) ||
                (baseOut < 2) || (baseOut > 36))
            {
                return result;
            }

            if (value.Trim().Length == 0)
            {
                return result;
            }

            // se baseIn e baseOut sono uguali la conversione è già fatta!
            if (baseIn == baseOut)
            {
                return value;
            }

            // determinazione del valore totale
            Double valore = 0;

            try
            {
                // se il campo è in base 10 non c'è bisogno di calcolare il valore
                if (baseIn == 10)
                {
                    valore = Double.Parse(value);
                }
                else
                {
                    Char[] c = value.ToCharArray();

                    // mi serve per l'elevazione a potenza e la trasformazione
                    // in valore base 10 della cifra
                    Int32 posizione = c.Length;

                    // ciclo sui caratteri di valueIn
                    // calcolo del valore decimale

                    for (Int32 k = 0; k < c.Length; k++)
                    {
                        // valore posizionale del carattere
                        Int32 valPos = codice.IndexOf(c[k]);

                        // verifica per caratteri errati
                        if ((valPos < 0) || (valPos > baseIn - 1))
                        {
                            return result;
                        }

                        posizione--;
                        valore += valPos * Math.Pow(baseIn, posizione);
                    }
                }

                // generazione del risultato final
                // se il risultato da generare è in base 10 non c'è
                // bisogno di calcoli
                if (baseOut == 10)
                {
                    result = valore.ToString();
                }

                else
                {
                    result = String.Empty;
                    
                    while (valore > 0)
                    {
                        Int32 resto = (Int32)(valore % baseOut);
                        valore = (valore - resto) / baseOut;
                        result = codice.Substring(resto, 1) + result;
                    }
                }
            }
            catch (Exception ex)
            {
                if(ex is OverflowException || ex is FormatException || ex is ArithmeticException)
                {
                    result = ex.Message;   
                }
                else
                {
                    throw;
                }
            }

            return result;
        }
    }
}