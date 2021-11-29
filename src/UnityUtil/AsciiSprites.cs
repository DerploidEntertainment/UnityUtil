using Sirenix.OdinInspector;
using System;

namespace UnityEngine {

    [CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.AsciiSprites)}", fileName = "ascii-sprites")]
    public class AsciiSprites : ScriptableObject {

        public static readonly int NumPrintables = 95;

        [HideInInspector]
        public string AutoLoadSpritePath = "ascii/{dec}.png";

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

        public Sprite Num0;
        public Sprite Num1;
        public Sprite Num2;
        public Sprite Num3;
        public Sprite Num4;
        public Sprite Num5;
        public Sprite Num6;
        public Sprite Num7;
        public Sprite Num8;
        public Sprite Num9;

        public Sprite Colon;
        public Sprite Semicolon;
        public Sprite Less;
        public Sprite Equal;
        public Sprite Greater;
        public Sprite Question;
        public Sprite AtSign;

        public Sprite UpperA;
        public Sprite UpperB;
        public Sprite UpperC;
        public Sprite UpperD;
        public Sprite UpperE;
        public Sprite UpperF;
        public Sprite UpperG;
        public Sprite UpperH;
        public Sprite UpperI;
        public Sprite UpperJ;
        public Sprite UpperK;
        public Sprite UpperL;
        public Sprite UpperM;
        public Sprite UpperN;
        public Sprite UpperO;
        public Sprite UpperP;
        public Sprite UpperQ;
        public Sprite UpperR;
        public Sprite UpperS;
        public Sprite UpperT;
        public Sprite UpperU;
        public Sprite UpperV;
        public Sprite UpperW;
        public Sprite UpperX;
        public Sprite UpperY;
        public Sprite UpperZ;

        public Sprite BracketOpen;
        public Sprite Backslash;
        public Sprite BracketClose;
        public Sprite Caret;
        public Sprite Underscore;
        public Sprite GraveAccent;

        [LabelText(nameof(LowerA))] public Sprite LowerA;
        [LabelText(nameof(LowerB))] public Sprite LowerB;
        [LabelText(nameof(LowerC))] public Sprite LowerC;
        [LabelText(nameof(LowerD))] public Sprite LowerD;
        [LabelText(nameof(LowerE))] public Sprite LowerE;
        [LabelText(nameof(LowerF))] public Sprite LowerF;
        [LabelText(nameof(LowerG))] public Sprite LowerG;
        [LabelText(nameof(LowerH))] public Sprite LowerH;
        [LabelText(nameof(LowerI))] public Sprite LowerI;
        [LabelText(nameof(LowerJ))] public Sprite LowerJ;
        [LabelText(nameof(LowerK))] public Sprite LowerK;
        [LabelText(nameof(LowerL))] public Sprite LowerL;
        [LabelText(nameof(LowerM))] public Sprite LowerM;
        [LabelText(nameof(LowerN))] public Sprite LowerN;
        [LabelText(nameof(LowerO))] public Sprite LowerO;
        [LabelText(nameof(LowerP))] public Sprite LowerP;
        [LabelText(nameof(LowerQ))] public Sprite LowerQ;
        [LabelText(nameof(LowerR))] public Sprite LowerR;
        [LabelText(nameof(LowerS))] public Sprite LowerS;
        [LabelText(nameof(LowerT))] public Sprite LowerT;
        [LabelText(nameof(LowerU))] public Sprite LowerU;
        [LabelText(nameof(LowerV))] public Sprite LowerV;
        [LabelText(nameof(LowerW))] public Sprite LowerW;
        [LabelText(nameof(LowerX))] public Sprite LowerX;
        [LabelText(nameof(LowerY))] public Sprite LowerY;
        [LabelText(nameof(LowerZ))] public Sprite LowerZ;

        public Sprite CurlyBraceOpen;
        public Sprite Pipe;
        public Sprite CurlyBraceClose;
        public Sprite Tilde;

        // API INTERFACE
        public Sprite this[char charCode] {
            get => spriteRef(charCode);
            set => spriteRef(charCode) = value;
        }
        public Sprite this[int number] {
            get {
                if (number < 0 || 9 < number)
                    throw new ArgumentOutOfRangeException(nameof(number));
                return spriteRef((char)('0' + number));
            }
            set {
                if (number < 0 || 9 < number)
                    throw new ArgumentOutOfRangeException(nameof(number));
                spriteRef((char)('0' + number)) = value;
            }
        }
        private ref Sprite spriteRef(char charCode) {
            switch (charCode) {
                case ' ': return ref Space;
                case '!': return ref Exclamation;
                case '"': return ref DoubleQuote;
                case '#': return ref Hashtag;
                case '$': return ref Dollar;
                case '%': return ref Percent;
                case '&': return ref Ampersand;
                case '\'': return ref Apostrophe;
                case '(': return ref ParenthesisOpen;
                case ')': return ref ParenthesisClose;
                case '*': return ref Asterisk;
                case '+': return ref Plus;
                case ',': return ref Comma;
                case '-': return ref Hyphen;
                case '.': return ref Period;
                case '/': return ref Slash;

                case '0': return ref Num0;
                case '1': return ref Num1;
                case '2': return ref Num2;
                case '3': return ref Num3;
                case '4': return ref Num4;
                case '5': return ref Num5;
                case '6': return ref Num6;
                case '7': return ref Num7;
                case '8': return ref Num8;
                case '9': return ref Num9;

                case ':': return ref Colon;
                case ';': return ref Semicolon;
                case '<': return ref Less;
                case '=': return ref Equal;
                case '>': return ref Greater;
                case '?': return ref Question;
                case '@': return ref AtSign;

                case 'A': return ref UpperA;
                case 'B': return ref UpperB;
                case 'C': return ref UpperC;
                case 'D': return ref UpperD;
                case 'E': return ref UpperE;
                case 'F': return ref UpperF;
                case 'G': return ref UpperG;
                case 'H': return ref UpperH;
                case 'I': return ref UpperI;
                case 'J': return ref UpperJ;
                case 'K': return ref UpperK;
                case 'L': return ref UpperL;
                case 'M': return ref UpperM;
                case 'N': return ref UpperN;
                case 'O': return ref UpperO;
                case 'P': return ref UpperP;
                case 'Q': return ref UpperQ;
                case 'R': return ref UpperR;
                case 'S': return ref UpperS;
                case 'T': return ref UpperT;
                case 'U': return ref UpperU;
                case 'V': return ref UpperV;
                case 'W': return ref UpperW;
                case 'X': return ref UpperX;
                case 'Y': return ref UpperY;
                case 'Z': return ref UpperZ;

                case '[': return ref BracketOpen;
                case '\\': return ref Backslash;
                case ']': return ref BracketClose;
                case '^': return ref Caret;
                case '_': return ref Underscore;
                case '`': return ref GraveAccent;

                case 'a': return ref LowerA;
                case 'b': return ref LowerB;
                case 'c': return ref LowerC;
                case 'd': return ref LowerD;
                case 'e': return ref LowerE;
                case 'f': return ref LowerF;
                case 'g': return ref LowerG;
                case 'h': return ref LowerH;
                case 'i': return ref LowerI;
                case 'j': return ref LowerJ;
                case 'k': return ref LowerK;
                case 'l': return ref LowerL;
                case 'm': return ref LowerM;
                case 'n': return ref LowerN;
                case 'o': return ref LowerO;
                case 'p': return ref LowerP;
                case 'q': return ref LowerQ;
                case 'r': return ref LowerR;
                case 's': return ref LowerS;
                case 't': return ref LowerT;
                case 'u': return ref LowerU;
                case 'v': return ref LowerV;
                case 'w': return ref LowerW;
                case 'x': return ref LowerX;
                case 'y': return ref LowerY;
                case 'z': return ref LowerZ;

                case '{': return ref CurlyBraceOpen;
                case '|': return ref Pipe;
                case '}': return ref CurlyBraceClose;
                case '~': return ref Tilde;

                default: throw new ArgumentOutOfRangeException(nameof(charCode), charCode, $"Can only return Sprites for the {NumPrintables} printable ASCII characters (codes 32-{32 + NumPrintables}), but character code '{charCode}' was requested!");
            };
        }

    }

}
