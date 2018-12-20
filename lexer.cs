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
	public enum TokType{
		NUMBER = 0, IDENT, ADD, SUB, MUL, DIV, MOD, NEG, SHL, SHR, 
		CONDL, CONDG, CONDLE, CONDGE, CONDE, CONDNE,
		DIRECTIVE
	}
	
	public abstract class Token
	{
		protected TokType tokenType;
		public TokType TokenType {
			get {
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
	
	public class SimpleToken: Token
	{
		public SimpleToken(TokType _tokenType)
		{
			tokenType = _tokenType;
		}
		
		public override String ToString(){
			return @"["+tokenType.ToString()+@"]";
		}
	}
	
	public class NumberToken: Token
	{
		private Int64 number;
		public Int64 Number{
			get{
				return number;
			}
		}
		
		public NumberToken(String str, Byte numBase)
		{
			tokenType = TokType.NUMBER;
			switch(numBase)
			{
				case 2:
					number = Convert.ToInt64(str.Substring(2),2);
					break;
				case 8:
					number = Convert.ToInt64(str.Substring(1),8);
					break;
				case 10:
					number = Convert.ToInt64(str,10);
					break;
				case 16:
					number = Convert.ToInt64(str.Substring(2),16);
					break;
			}
		}
		
		public override String ToString(){
			return @"["+tokenType.ToString()+"] = \"" + number.ToString() + "\"";
		}
		
	}
	
	public class IdentToken: Token
	{
		private String ident;
		public string Ident {
			get {
				return ident;
			}
		}

		public IdentToken(String _ident)
		{
			ident = _ident;
			tokenType = TokType.IDENT;
		}
		
		public override String ToString(){
			return @"["+tokenType.ToString()+"] = \"" + ident + "\"";	
		}
	}
	
	public class Program
	{
		private static List<Token> commandList = new List<Token>();
		private static StringBuilder tmp;
		private static Int32 i;
		
		
		public static void Main(string[] args)
		{
			string s = "24+3;\n" +
				"28 / (5 - 56) + 10-(5+8*ggg)* -0b10010\n" +
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
			return ("<>=(){}*/+-~%;!".IndexOf(ch) != -1)||char.IsWhiteSpace(ch);
		}
		
		
		private static void ListFromString(String code)
        {
            StringBuilder tmp = new StringBuilder();
			code.Replace('\t', ' ');
            i = 0;
            Byte numberBase; 
            try
            {
                while (i < code.Length)
                {
                	// Удаляем пробелы
					while (i < code.Length && Char.IsWhiteSpace(code[i]))
						++i;
                    
					// Нашли букву - считываем идентификатор в буфер
					if(i < code.Length && (char.IsLetter(code,i) || code[i] == '_')){
						tmp.Append(code[i++]);
						// Тут можно уже считывать и числа и буквы
						while(i < code.Length && (char.IsDigit(code,i) || char.IsLetter(code,i) || code[i] == '_'))
	            			tmp.Append(code[i++]);
					}
	            	// Если что-то нашли при поиске идентификатора - сохраняем
	            	if(tmp.Length > 0){
	            		commandList.Add(new IdentToken(tmp.ToString()));
	            		tmp.Length = 0; // И очищаем буфер
	            	}else if(i < code.Length)
	            	{
	            	//Ищем числа
		            	if(char.IsDigit(code,i))
		            	{
		            		if(i+1 < code.Length && code[i] == 0){
		            			
		            			if(code[i+1] == 'x')
			            		{
		            				numberBase = 16;
		            				while(i < code.Length && code[i] > '0' && code[i] <= '9' 
		            				      && char.ToUpper(code[i]) > 'A' && char.ToUpper(code[i]) <= 'F')
		            					tmp.Append(code[i++]);
		            			}else if(code[i+1] == 'b'){
		            				numberBase = 2;
		            				while(i < code.Length && code[i] == '0' && code[i] == '1')
		            					tmp.Append(code[i++]);
		            			}else{
		            				numberBase = 8;
		            				while(i < code.Length && code[i] > '0' && code[i] <= '7')
		            					tmp.Append(code[i++]);
		            			}
		            		}else{
		            			numberBase = 10;
		            			while(char.IsDigit(code,i)){
		            				tmp.Append(code[i++]);
		            			}
		            		}
		            		

		            		if(tmp.Length > 0 && IsDelimeter(code[i])){
		            			commandList.Add(new NumberToken(tmp.ToString(),numberBase));
		            			tmp.Length = 0;
		            		}	
				
		            	}else if(code[i] == '>'){// Далее пытаемся поймать операторы
		            		if(i+1 < code.Length && code[i+1]=='='){
		            			commandList.Add(new SimpleToken(TokType.CONDGE));
		            			i += 2;             
		            		}else{
		            			commandList.Add(new SimpleToken(TokType.CONDG));
		            			++i;
		            		}
		            	}else if(code[i] == '<'){
		            		if(i+1 < code.Length && code[i+1]=='='){
		            			commandList.Add(new SimpleToken(TokType.CONDLE));
		            			i += 2;             
		            		}else{
		            			commandList.Add(new SimpleToken(TokType.CONDL));
		            			++i;
		            		}	
		            	}else if(code[i] == '!'){
		            		if(i+1 < code.Length && code[i+1]=='='){
		            			commandList.Add(new SimpleToken(TokType.CONDNE));
		            			i += 2;             
		            		}else{
		            			commandList.Add(new SimpleToken(TokType.NEG));
		            			++i;
		            		}
		            	}else if (IsDelimeter(code[i]))
		                {
		            		if (code.Length >= i+1 && code[i] == '/' && code[i+1] == '/') // пропускаем комментарии
							{
								int id = code.IndexOf('\n');
								i = code.IndexOf('\n', i) + 1;
		            		}else{
		            			commandList.Add(new SimpleToken(TokType.ADD));
		            			++i;
		            		}
		                }
		                else throw new Exception("Какаято херня у вас происходит.....");
	            	}
	
                }

            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Возможна ошибка преобразования строки в список команд!\nПояснения в следующем окне.");
				System.Console.WriteLine(ex.ToString());
                return;
            }
        }
	}
}
