
/*
 * Created by SharpDevelop.
 * User: Касаткин
 * Date: 13.12.2018
 * Time: 14:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace lexer
{
    public enum TokType
    {
        NUMBER = 0, IDENT, ADD, SUB, MUL, DIV, MOD, NEG, SHL, SHR, ASSIGN,
        CONDL, CONDG, CONDLE, CONDGE, CONDE, CONDNE, IF, ELSE, RFBRACE, LFBRACE, RRBRACE, LRBRACE,
        DIRECTIVE
    }

    public abstract class Token
    {
        protected TokType tokenType;
        public TokType TokenType
        {
            get
            {
                return tokenType;
            }
        }
        public bool IsTokOfType(TokType _tokenType)
        {
            return tokenType == _tokenType;
        }

        public bool IsOperator()
        {
            return (tokenType >= TokType.ADD) && (TokenType <= TokType.SHR);
        }

        public bool IsOperand()
        {
            return tokenType <= TokType.IDENT;
        }
        public abstract override String ToString();
    }

    public class SimpleToken : Token
    {
        public SimpleToken(TokType _tokenType)
        {
            tokenType = _tokenType;
        }

        public override String ToString()
        {
            return @"[" + tokenType.ToString() + @"]";
        }
    }

    public class NumberToken : Token
    {
        private Int64 number;
        public Int64 Number
        {
            get
            {
                return number;
            }
        }

        public NumberToken(String str, Byte numBase)
        {
            tokenType = TokType.NUMBER;
            switch (numBase)
            {
                case 2:
                    number = Convert.ToInt64(str.Substring(2), 2);
                    break;
                case 8:
                    number = Convert.ToInt64(str.Substring(1), 8);
                    break;
                case 10:
                    number = Convert.ToInt64(str, 10);
                    break;
                case 16:
                    number = Convert.ToInt64(str.Substring(2), 16);
                    break;
            }
        }

        public override String ToString()
        {
            return @"[" + tokenType.ToString() + "] = \"" + number.ToString() + "\"";
        }

    }

    public class IdentToken : Token
    {
        private String ident;
        public string Ident
        {
            get
            {
                return ident;
            }
        }

        public IdentToken(String _ident)
        {
            ident = _ident;
            tokenType = TokType.IDENT;
        }

        public override String ToString()
        {
            return @"[" + tokenType.ToString() + "] = \"" + ident + "\"";
        }
    }

    public class Program
    {
        private static List<Token> commandList = new List<Token>();
        private static StringBuilder tmp;
        private static Int32 i;


        public static void Main(string[] args)
        {
            string s = "a=5-(0b10010+c)*b+(a!=10)\n" +
                "//gbfgbfgbfg gbft b fgb fgb f\n" +
                "gbfgb + g\n";

            ListFromString(s);

            foreach (Token token in commandList)
                Console.WriteLine(token);
            Console.WriteLine("Press any key to continue . . . ");
            Console.ReadKey(true);

        }

        private static bool IsDelimeter(char ch)
        {
            return ("<>=(){}*/+-~%;!".IndexOf(ch) != -1) || char.IsWhiteSpace(ch);
        }


        private static void ListFromString(String code)
        {
            StringBuilder tmp = new StringBuilder();
            code.Replace('\t', ' ');
            i = 0;
            int _ = 5;
            Byte numberBase;
            try
            {
                while (i < code.Length)
                {
                    // Нашли букву - считываем идентификатор в буфер
                    if (char.IsLetter(code, i) || code[i] == '_')
                    {
                        tmp.Append(code[i++]);
                        while (i < code.Length && (char.IsDigit(code, i) || char.IsLetter(code, i) || code[i] == '_'))
                            tmp.Append(code[i++]);
                        if (tmp.Length > 0)
                        {
                            commandList.Add(new IdentToken(tmp.ToString()));
                            tmp.Length = 0; // И очищаем буфер
                        }
                    }  else if (char.IsDigit(code, i))
                    {
                            if (i + 1 < code.Length & code[i] == '0')
                            {
                                if (code[++i] == 'x')
                                {
                                    ++i;
                                    numberBase = 16;
                                    while (char.IsDigit(code, i) && char.ToUpper(code[i]) > 'A' && char.ToUpper(code[i]) <= 'F')
                                        tmp.Append(code[i++]);
                                }
                                else if (code[i] == 'b')
                                {
                                    ++i;
                                    numberBase = 2;
                                    while (i < code.Length && code[i] == '0' && code[i] == '1')
                                        tmp.Append(code[i++]);
                                }
                                else
                                {
                                    ++i;
                                    numberBase = 8;
                                    while (i < code.Length && code[i] > '0' && code[i] <= '7')
                                        tmp.Append(code[i++]);
                                }
                            } else {
                                numberBase = 10;
                                while (char.IsDigit(code, i))
                                    tmp.Append(code[i++]);
                            }

                            if (tmp.Length > 0 && IsDelimeter(code[i]))
                            {
                                commandList.Add(new NumberToken(tmp.ToString(), numberBase));
                                tmp.Length = 0;
                            }

                    } else switch (code[i]) {
                        case '+':
                            commandList.Add(new SimpleToken(TokType.ADD));
                            break;
                        case '-':
                            commandList.Add(new SimpleToken(TokType.SUB));
                            break;
                        case '*':
                            commandList.Add(new SimpleToken(TokType.MUL));
                            break;
                        case '/':
                            commandList.Add(new SimpleToken(TokType.DIV));
                            break;
                        case '%':
                            commandList.Add(new SimpleToken(TokType.MOD));
                            break;
                        case '~':
                            commandList.Add(new SimpleToken(TokType.NEG));
                            break;
                        case '{':
                            commandList.Add(new SimpleToken(TokType.LFBRACE));
                            break;
                        case '(':
                            commandList.Add(new SimpleToken(TokType.LRBRACE));
                            break;
                        case '}':
                                commandList.Add(new SimpleToken(TokType.RFBRACE));
                            break;
                        case ')':
                                commandList.Add(new SimpleToken(TokType.RRBRACE));
                            break;
                        case '>':
                            if (++i < code.Length & code[i] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDGE));
                                ++i;
                            } else
                                commandList.Add(new SimpleToken(TokType.CONDG));
                            break;
                        case '<':
                            if (++i < code.Length & code[i] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDLE));
                                ++i;
                            } else  
                                commandList.Add(new SimpleToken(TokType.CONDL));
                            break;
                        case '!':
                            if (++i < code.Length & code[i] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDNE));
                                ++i;
                            } else 
                                commandList.Add(new SimpleToken(TokType.NEG));
                            break;
                        case '=':
                            if (++i < code.Length & code[i] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDE));
                                ++i;
                            } else
                                commandList.Add(new SimpleToken(TokType.ASSIGN));
                            break;
                        default:
                                if (IsDelimeter(code[i]))
                                {
                                    if (code.Length >= i+1 && code[i] == '/' && code[i+1] == '/') // пропускаем комментарии
                                        i = code.IndexOf('\n', i) + 1;
                                    else
                                    {
                                        commandList.Add(new SimpleToken(TokType.ADD));
                                        ++i;
                                    }
                                }
                                else if (Char.IsWhiteSpace(code[i]))
                                    while ((++i < code.Length) & Char.IsWhiteSpace(code[i])) { }
                                else
                                    throw new Exception("Какаято херня у вас происходит.....");
                                break;

                    }

                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Возможна ошибка преобразования строки в список команд!\nПояснения в следующем окне.");
                System.Console.WriteLine(ex.ToString());
                return;
            }
        }
    }
}
