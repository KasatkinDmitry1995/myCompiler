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
	class Program
	{
		private static List<String> cmd_list = new List<String>();
		
		public static void Main(string[] args)
		{
			string s = "24+3;" +
				"28 / (5 - 56) + 10-(5+8*ggg)+0b10010";

			ListFromString(s);
			foreach (string token in cmd_list)
			Console.WriteLine(token);	
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
			
		}
		
		private static bool IsDelimeter(char ch)
		{
			return ("<>=(){}*/+-~%;!".IndexOf(ch) != -1)&&char.IsWhiteSpace(ch);
		}
		
		private static void ListFromString(String code)
        {
            cmd_list.Clear();
            StringBuilder tmp = new StringBuilder();
			code.Replace('\t', ' ');
            Int32 i = 0;
            try
            {
                while (i < code.Length)
                {
                	// Удаляем пробелы
					while (i < code.Length && Char.IsWhiteSpace(code[i]))
						++i;
					
					// Обработка комментраиев
					if (code[i] == '/' && code[i+1] == '/')
					{
						int id = code.IndexOf('\n');
						i = code.IndexOf('\n', i) + 1;
					}
					
					// Закончилась строка
					if (code[i] == '\n')
					{
						// Если есть что добавлять то добавляем
						if (tmp.Length!=0)
						{
							cmd_list.Add(tmp.ToString());
							tmp.Length = 0;
						}
						++i;
					}
					
					// Снова удаляем пробелы
                    while (i < code.Length && Char.IsWhiteSpace(code, i))
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
	            		cmd_list.Add(tmp.ToString());
	            		tmp.Length = 0; // И очищаем буфер
	            	}else if(i < code.Length)
	            	{
	            	//Ищем числа
		            	if(char.IsDigit(code,i))
		            	{
		            		if(i+1 < code.Length && code[i] == 0){
		            			
		            			if(code[i+1] == 'x')
			            		{
		            				while(i < code.Length && code[i] > '0' && code[i] <= '9' 
		            				      && char.ToUpper(code[i]) > 'A' && char.ToUpper(code[i]) <= 'F')
		            					tmp.Append(code[i++]);
		            			}else if(code[i+1] == 'b'){
		            				while(i < code.Length && code[i] == '0' && code[i] == '1')
		            					tmp.Append(code[i++]);
		            			}else{
		            				while(i < code.Length && code[i] > '0' && code[i] <= '7')
		            					tmp.Append(code[i++]);
		            			}
		            		}else{
		            			while(char.IsDigit(code,i)){
		            				tmp.Append(code[i++]);
		            			}
		            		}
		            		
		            		if(tmp.Length > 0 && IsDelimeter(code[i])){
		            			cmd_list.Add(tmp.ToString());
		            			tmp.Length = 0;
		            		}	
				
		            	}else if(code[i] == '>'){// Далее пытаемся поймать операторы
		            		if(i+1 < code.Length && code[i+1]=='='){
		            			cmd_list.Add(">=");
		            			i += 2;             
		            		}else{
		            			cmd_list.Add(">");
		            			++i;
		            		}	
		            	}else if(code[i] == '<'){
		            		if(i+1 < code.Length && code[i+1]=='='){
		            			cmd_list.Add("<=");
		            			i += 2;             
		            		}else{
		            			cmd_list.Add("<");
		            			++i;
		            		}	
		            	}else if(code[i] == '!'){
		            		if(i+1 < code.Length && code[i+1]=='='){
		            			cmd_list.Add("!=");
		            			i += 2;             
		            		}else{
		            			cmd_list.Add("!");
		            			++i;
		            		}	
		            	}else if (@"{}()[]/*=+-;".IndexOf(code[i]) != -1)
		                {
		                	// Тут возможно сделать определение отрицательного числа
		                	// Для этого усложним лексер. Определим может ли быть унарный минус.
		                	// Если может быть унарный минус, то если за ним число сразу - лепим его с числом
		                	// Если не число (да, пробел тоже все отменит) - то это уначный минус
		     
		                    cmd_list.Add(code[i++].ToString());
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
