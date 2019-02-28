using System;
using System.Collections.Generic;
using RTools_NTS.Util;

namespace NetTopologySuite.Utilities
{
    internal sealed class TokenStream
    {
        private bool? prevMoveNextResult;

        private Token nextToken;

        public TokenStream(IEnumerator<Token> enumerator) => this.Enumerator = enumerator;

        public IEnumerator<Token> Enumerator { get; }

        public Token NextToken(bool advance)
        {
            if (this.prevMoveNextResult == null)
            {
                this.ReadNextToken();
            }

            var result = this.nextToken;
            if (advance)
            {
                this.ReadNextToken();
            }

            return result;
        }

        private void ReadNextToken()
        {
            if (this.Enumerator.MoveNext())
            {
                this.prevMoveNextResult = true;
                this.nextToken = this.Enumerator.Current;
                if (this.nextToken == null)
                {
                    throw new InvalidOperationException("Token list contains a null value.");
                }
            }
            else
            {
                this.prevMoveNextResult = false;
                this.nextToken = null;
            }
        }
    }
}
