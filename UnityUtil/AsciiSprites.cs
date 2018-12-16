using System;

namespace UnityEngine {

    [CreateAssetMenu(menuName = nameof(UnityEngine) + "/" + nameof(UnityEngine.AsciiSprites), fileName = "ascii-sprites")]
    public class AsciiSprites : ScriptableObject {

        private const int NUM_PRINTABLES = 95;

        public Sprite Space;
        public Sprite Exclamation;
        public Sprite DoubleQuote;
        public Sprite Hashtag;
        public Sprite Dollar;
        public Sprite Percent;
        public Sprite Ampersand;
        public Sprite Apostrophe;
        public Sprite ParenthesisOpen;
        public Sprite ParenthesisClose;
        public Sprite Asterisk;
        public Sprite Plus;
        public Sprite Comma;
        public Sprite Hyphen;
        public Sprite Period;
        public Sprite Slash;
        public Sprite Zero;
        public Sprite One;
        public Sprite Two;
        public Sprite Three;
        public Sprite Four;
        public Sprite Five;
        public Sprite Six;
        public Sprite Seven;
        public Sprite Eight;
        public Sprite Nine;
        public Sprite Colon;
        public Sprite Semicolon;
        public Sprite Less;
        public Sprite Equal;
        public Sprite Greater;
        public Sprite Question;
        public Sprite AtSign;

        public Sprite A;
        public Sprite B;
        public Sprite C;
        public Sprite D;
        public Sprite E;
        public Sprite F;
        public Sprite G;
        public Sprite H;
        public Sprite I;
        public Sprite J;
        public Sprite K;
        public Sprite L;
        public Sprite M;
        public Sprite N;
        public Sprite O;
        public Sprite P;
        public Sprite Q;
        public Sprite R;
        public Sprite S;
        public Sprite T;
        public Sprite U;
        public Sprite V;
        public Sprite W;
        public Sprite X;
        public Sprite Y;
        public Sprite Z;

        public Sprite BracketOpen;
        public Sprite Backslash;
        public Sprite BracketClose;
        public Sprite Caret;
        public Sprite Underscore;
        public Sprite GraveAccent;

        #pragma warning disable IDE1006 // Naming Styles
        public Sprite a;
        public Sprite b;
        public Sprite c;
        public Sprite d;
        public Sprite e;
        public Sprite f;
        public Sprite g;
        public Sprite h;
        public Sprite i;
        public Sprite j;
        public Sprite k;
        public Sprite l;
        public Sprite m;
        public Sprite n;
        public Sprite o;
        public Sprite p;
        public Sprite q;
        public Sprite r;
        public Sprite s;
        public Sprite t;
        public Sprite u;
        public Sprite v;
        public Sprite w;
        public Sprite x;
        public Sprite y;
        public Sprite z;
        #pragma warning restore IDE1006 // Naming Styles

        public Sprite CurlyBraceOpen;
        public Sprite Pipe;
        public Sprite CurlyBraceClose;
        public Sprite Tilde;

        // API INTERFACE
        public Sprite this[char charCode] {
            get {
                switch (charCode) {
                    case ' ': return Space;
                    case '!': return Exclamation;
                    case '"': return DoubleQuote;
                    case '#': return Hashtag;
                    case '$': return Dollar;
                    case '%': return Percent;
                    case '&': return Ampersand;
                    case '\'': return Apostrophe;
                    case '(': return ParenthesisOpen;
                    case ')': return ParenthesisClose;
                    case '*': return Asterisk;
                    case '+': return Plus;
                    case ',': return Comma;
                    case '-': return Hyphen;
                    case '.': return Period;
                    case '/': return Slash;
                    case '0': return Zero;
                    case '1': return One;
                    case '2': return Two;
                    case '3': return Three;
                    case '4': return Four;
                    case '5': return Five;
                    case '6': return Six;
                    case '7': return Seven;
                    case '8': return Eight;
                    case '9': return Nine;
                    case ':': return Colon;
                    case ';': return Semicolon;
                    case '<': return Less;
                    case '=': return Equal;
                    case '>': return Greater;
                    case '?': return Question;
                    case '@': return AtSign;
                    case 'A': return A;
                    case 'B': return B;
                    case 'C': return C;
                    case 'D': return D;
                    case 'E': return E;
                    case 'F': return F;
                    case 'G': return G;
                    case 'H': return H;
                    case 'I': return I;
                    case 'J': return J;
                    case 'K': return K;
                    case 'L': return L;
                    case 'M': return M;
                    case 'N': return N;
                    case 'O': return O;
                    case 'P': return P;
                    case 'Q': return Q;
                    case 'R': return R;
                    case 'S': return S;
                    case 'T': return T;
                    case 'U': return U;
                    case 'V': return V;
                    case 'W': return W;
                    case 'X': return X;
                    case 'Y': return Y;
                    case 'Z': return Z;
                    case '[': return BracketOpen;
                    case '\\': return Backslash;
                    case ']': return BracketClose;
                    case '^': return Caret;
                    case '_': return Underscore;
                    case '`': return GraveAccent;
                    case 'a': return a;
                    case 'b': return b;
                    case 'c': return c;
                    case 'd': return d;
                    case 'e': return e;
                    case 'f': return f;
                    case 'g': return g;
                    case 'h': return h;
                    case 'i': return i;
                    case 'j': return j;
                    case 'k': return k;
                    case 'l': return l;
                    case 'm': return m;
                    case 'n': return n;
                    case 'o': return o;
                    case 'p': return p;
                    case 'q': return q;
                    case 'r': return r;
                    case 's': return s;
                    case 't': return t;
                    case 'u': return u;
                    case 'v': return v;
                    case 'w': return w;
                    case 'x': return x;
                    case 'y': return y;
                    case 'z': return z;
                    case '{': return CurlyBraceOpen;
                    case '|': return Pipe;
                    case '}': return CurlyBraceClose;
                    case '~': return Tilde;
                    default: throw new ArgumentOutOfRangeException(nameof(charCode), charCode, $"Can only return Sprites for the {NUM_PRINTABLES} printable ASCII characters (codes 32-{32 + NUM_PRINTABLES}), but character code '{charCode}' was requested!");
                }
            }
        }

    }

}
