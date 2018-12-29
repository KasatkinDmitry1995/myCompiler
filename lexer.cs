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
        CONDL, CONDG, CONDLE, CONDGE, CONDE, CONDNE, 
        IF, ELSE, WHILE, DO, SWITCH, FOR, CASE, BREAK, CONTINUE, PROCEDURE, FUNCTION,
        RFBRACE, LFBRACE, RRBRACE, LRBRACE, SEMICOLON, COMMA
    }

    public abstract class Token
    {
        protected TokType tokenType;
        protected Int32 line;
        public TokType TokenType
        {
            get
            {
                return tokenType;
            }
        }
        public Int32 Line
        {
            get
            {
                return line;
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
        
        public static readonly Dictionary<String, TokType> Directives = new Dictionary<string, TokType>()
		{
			{@"if",TokType.IF},
			{@"else",TokType.ELSE},
			{@"while",TokType.WHILE},
			{@"do",TokType.DO},
			{@"for",TokType.FOR},
			{@"switch",TokType.SWITCH},
			{@"case",TokType.CASE},
			{@"break",TokType.BREAK},
			{@"continue",TokType.CONTINUE},
			{@"procedure",TokType.PROCEDURE},
			{@"function",TokType.FUNCTION}
		};
        
        public static Token GetVarOrDirective(String str, Int32 line){
        	if(Directives.ContainsKey(str)){
        		return new SimpleToken(Directives[str], line);
        	}
        	return new IdentToken(str, line);
        }
    }

    public class SimpleToken : Token
    {
        public SimpleToken(TokType _tokenType, Int32 _line)
        {
            tokenType = _tokenType;
            line = _line;
        }

        public override String ToString()
        {
        	return line.ToString() +@" >> [" + tokenType.ToString() + @"]";
        }
    }

    public class NumberToken : Token
    {
        private Int64 number;
        public Int64 Number
        {
            get { return number; }
        }

        public NumberToken(String str, Byte numBase, Int32 _line)
        {
            tokenType = TokType.NUMBER;
            line = _line;
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
            return line.ToString() +@" >> [" + tokenType.ToString() + "] = \"" + number.ToString() + "\"";
        }

    }

    public class IdentToken : Token
    {
        private String ident;
        public string Ident
        {
            get {  return ident; }
        }

        public IdentToken(String _ident, Int32 _line)
        {
            ident = _ident;
            line = _line;
            tokenType = TokType.IDENT;
        }

        public override String ToString()
        {
            return line.ToString() +@" >> [" + tokenType.ToString() + "] = \"" + ident + "\"";
        }
    }

    public class Program
    {
    	public static void Main(string[] args)
        {
            string s = "a?=5;\n" +
                "?a =  b-3?;//gbfgbfgbfg gbft b fgb fgb f\n" +
                "?  \n" +
            	"gbfgb + g;\n" +
            	"function func(a, b, c)\n" +
            	"{\n" +
            	"	return a + b + c;\n" +
            	"}\n" +
            	"//comment\n" +
            	"func(3,2,8)\n" +
            	"\n" +
            	"";

            Lexer lex = new Lexer();
            lex.Tokenize(s);
			
            while(lex.Next())
            {
            	Console.WriteLine(lex.GetCurrentToken());
            }
            
            if(lex.HasErrors()){
               	int l = lex.GetErrorsCount(), i = 0;
               	while(i < l)
               		Console.WriteLine(lex.GetError(i++));
            }

            Console.WriteLine("Press any key to continue . . . ");
            Console.ReadKey(true);

        }	
    }
    
    public class Lexer
    {
        private List<Token> commandList = new List<Token>();
        private List<String> errorList = new List<string>();
        public int currentTok;

        public Token GetNextToken(bool eat)
        {
        	if (currentTok < commandList.Count)
        	{
        		if(eat)
        			return commandList[currentTok++];
        		else
        			return commandList[currentTok];
        	}else 
        		return null;        		
        }
        
        public Token GetCurrentToken()
        {
        	if (currentTok < 0 || currentTok >= commandList.Count)
        		return null;
        	else	
        		return commandList[currentTok];
        }
        
        public bool Next()
        {
        	if (currentTok < commandList.Count){
				currentTok++;
				return true;
        	}
        	else 
        		return false;		
        }
        
        public bool HasMoreTokens()
        {
        	return currentTok < commandList.Count;	
        }
        
        public bool HasErrors()
        {
        	return errorList.Count != 0;
        }
        
        public String GetError(int n)
        {
        	if(n<errorList.Count)
        		return errorList[n];
        	else
        		return null;
        }
        
        public int GetErrorsCount(){
        	return errorList.Count;
        }
        
        public void Tokenize(String code)
        {
        	commandList.Clear();
        	errorList.Clear();
        	currentTok = -1;
            StringBuilder tmp = new StringBuilder();
            Int32 line = 1, i = 0, last_i = -1;
            Byte numberBase;

            while (i < code.Length)
            {
                // Нашли букву - считываем идентификатор в буфер
                if (char.IsLetter(code, i) || code[i] == '_')
                {
                    tmp.Append(code[i++]);
                    while (i < code.Length && (char.IsDigit(code, i) || char.IsLetter(code, i) || code[i] == '_'))
                        tmp.Append(code[i++]);

                    commandList.Add(Token.GetVarOrDirective(tmp.ToString(), line));
                    tmp.Length = 0; // очищаем буфер
                }else if (char.IsDigit(code, i)) 
                {
                        if (i + 1 < code.Length & code[i] == '0')
                        {
                            if (code[++i] == 'x')
                            {
                                ++i;
                                numberBase = 16;
                                while (char.IsDigit(code, i) && char.ToUpper(code[i]) > 'A' && char.ToUpper(code[i]) <= 'F')
                                    tmp.Append(code[i++]);
                            } else if (code[i] == 'b')
                            {
                                ++i;
                                numberBase = 2;
                                while (i < code.Length && code[i] == '0' && code[i] == '1')
                                    tmp.Append(code[i++]);
                            } else
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
                        commandList.Add(new NumberToken(tmp.ToString(), numberBase, line));
                        tmp.Length = 0;
                        
                } else {
                	switch (code[i]) {
                    	
                        case '+':
                            commandList.Add(new SimpleToken(TokType.ADD, line));
                            break;
                        case '-':
                            commandList.Add(new SimpleToken(TokType.SUB, line));
                            break;
                        case '*':
                            commandList.Add(new SimpleToken(TokType.MUL, line));
                            break;
                        case '%':
                            commandList.Add(new SimpleToken(TokType.MOD, line));
                            break;
                        case '~':
                            commandList.Add(new SimpleToken(TokType.NEG, line));
                            break;
                        case '{':
                            commandList.Add(new SimpleToken(TokType.LFBRACE, line));
                            break;
                        case '(':
                            commandList.Add(new SimpleToken(TokType.LRBRACE, line));
                            break;
                        case '}':
                            commandList.Add(new SimpleToken(TokType.RFBRACE, line));
                            break;
                        case ')':
                            commandList.Add(new SimpleToken(TokType.RRBRACE, line));
                            break;
                        case ',':
                            commandList.Add(new SimpleToken(TokType.COMMA, line));
                            break;
                        case '>':
                            if (i+1 < code.Length & code[i+1] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDGE, line));
								++i;
                            } else
                                commandList.Add(new SimpleToken(TokType.CONDG, line));
                            break;
                        case '<':
                            if (i+1 < code.Length & code[i+1] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDLE, line));
                                ++i;
                            } else  
                                commandList.Add(new SimpleToken(TokType.CONDL, line));
                            break;
                        case '!':
                            if (i+1 < code.Length & code[i+1] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDNE, line));
                                ++i;
                            } else 
                                commandList.Add(new SimpleToken(TokType.NEG, line));
                            break;
                        case '=':
                            if (i+1 < code.Length & code[i+1] == '=')
                            {
                                commandList.Add(new SimpleToken(TokType.CONDE, line));
                                ++i;
                            } else 
                                commandList.Add(new SimpleToken(TokType.ASSIGN, line));
                            break;
                        case '/':
                         	if (i+1 < code.Length & code[i+1] == '/')
                            {
                         		i = code.IndexOf('\n', i);
                         		if(i == -1){
                         			i = code.Length;
                         			continue;
                         		}else {
                         			++line;
                         			last_i = i;
                         		}
                            } else 
                                commandList.Add(new SimpleToken(TokType.DIV, line));
                            break; 
                        case ';':
                            commandList.Add(new SimpleToken(TokType.SEMICOLON, line));
                            break;
                        case ' ':
                        case '\r':
                        case '\t':
                            while ((++i < code.Length) & 
                                   (code[i] == ' ' || code[i] == '\r' || code[i] == '\t')) 
                            	{ }
                            continue;
                        case '\n':
                        	++line;
                        	last_i = i;
                        	break;
                        default:
                        	errorList.Add("Неожиданный символ в исходном тексте программы, на строке " + 
                        					line.ToString() + " на позиции " + (i - last_i).ToString());
                        	break;
                    }
                ++i;
                }
            }

        }
    }
}
