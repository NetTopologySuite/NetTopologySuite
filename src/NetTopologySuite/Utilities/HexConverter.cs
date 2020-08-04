using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public class HexConverter
    {
        /// <summary>
        /// Only static methods!
        /// </summary>
        private HexConverter() { }

        /// <summary>
        /// Convert the given numeric value (passed as string) of the base specified by <c>baseIn</c>
        /// to the value specified by <c>baseOut</c>.
        /// </summary>
        /// <param name="valueIn">Numeric value to be converted, as string.</param>
        /// <param name="baseIn">Base of input value.</param>
        /// <param name="baseOut">Base to use for conversion.</param>
        /// <returns>Converted value, as string.</returns>
        public static string ConvertAny2Any(string valueIn, int baseIn, int baseOut)
        {
              string result = "Error";

              valueIn = valueIn.ToUpper();
              const string codice = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

              // test per limite errato sulle basi in input e/o in output
             if ((baseIn < 2) || (baseIn > 36) ||
                  (baseOut < 2) || (baseOut > 36))
                            return result;

             if (valueIn.Trim().Length == 0)
                           return result;

             // se baseIn e baseOut sono uguali la conversione è già fatta!
             if (baseIn == baseOut)
                          return valueIn;

             // determinazione del valore totale
             double valore = 0;
             try
             {
                  // se il campo è in base 10 non c'è bisogno di calcolare il valore
                  if (baseIn == 10)
                      valore = double.Parse(valueIn);
                  else
                  {
                        char[] c = valueIn.ToCharArray();

                        // mi serve per l'elevazione a potenza e la trasformazione
                        // in valore base 10 della cifra
                        int posizione = c.Length;

                        // ciclo sui caratteri di valueIn
                        // calcolo del valore decimale

                        for (int k = 0; k < c.Length; k++)
                        {
                              // valore posizionale del carattere
                              int valPos = codice.IndexOf(c[k]);

                              // verifica per caratteri errati
                              if ((valPos < 0) || (valPos > baseIn - 1))
                                         return result;

                              posizione--;
                              valore += valPos * Math.Pow(baseIn, posizione);
                        }
                  }

                  // generazione del risultato final
                  // se il risultato da generare è in base 10 non c'è
                  // bisogno di calcoli
                  if (baseOut == 10)
                           result = valore.ToString();

                  else
                  {
                           result = string.Empty;
                           while (valore > 0)
                          {
                                int resto = (int) (valore % baseOut);
                                valore = (valore - resto) / baseOut;
                                result = codice.Substring(resto,1) + result;
                           }
                  }

             }
             catch (Exception ex)
             {
                      result = ex.Message;
             }
             return result;
        }
    }
}
