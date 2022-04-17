using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace uniin
{
    class Program
    {
        static void Main(string[] args)
        {
            Menu menu = new Menu();
        }
    }
    public enum Language
    {
        Unknown = 0,
        English,
        Numbers
    }

    public class Menu
    {
        public Account MyAccount;
        public AllQuizzez MyAllQuizzez;
        public static string PathAccs;
        public Dictionary<string, string> Accs;
        public static string PathStats;
        public Dictionary<KeyValuePair<string, string>, int> Statistics;

        static Menu()
        {
            PathAccs = "Accs.txt";
            PathStats = "stats.txt";
        }

        public Menu()
        {
            MyAllQuizzez = new AllQuizzez();
            Accs = new Dictionary<string, string>();
            Statistics = new Dictionary<KeyValuePair<string, string>, int>();
            using (FileStream fileStream = new FileStream(PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexAccs = new Regex(@"[A-z](\w*)");
                    Regex regexPasswords = new Regex(@"[0-9](\w*)");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        Match matchAccs = regexAccs.Match(currentLine);
                        Match matchPasswords = regexPasswords.Match(currentLine);
                        Accs.Add(matchAccs.Value, matchPasswords.Value);
                    }
                }
            }

            using (FileStream fileStream = new FileStream(PathStats, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexQuizzez = new Regex("\".*?\"");
                    Regex regexLogins = new Regex(@"[A-z](\w*)");
                    Regex regexTryAnswers = new Regex(@"[0-9](\w*)");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        Match matchQuizzez = regexQuizzez.Match(currentLine);
                        Match matchLogins = regexLogins.Match(currentLine);
                        Match matchTryAnswers = regexTryAnswers.Match(currentLine);
                        Statistics.Add(new KeyValuePair<string, string>(matchQuizzez.Value.Trim('"'), matchLogins.Value), Convert.ToInt32(matchTryAnswers.Value));
                    }
                }
            }
            ShowLoginOrRegister(); // внутри будет инициализирован MyAccount
        }

        public bool EnterLogin(out string login) // проверяет на латиницу, на существование такого же и иниц. логин
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите ваш логин: ");
                login = Console.ReadLine();
                if (Account.IsEnglishLanguage(login) == false)
                    continue;
                if (Accs.ContainsKey(login) == false)
                {
                    Console.Clear();
                    Console.WriteLine("Данный логин не зарегистрирован!");
                    Console.ReadKey();
                    return false;
                }
                break;
            }
            while (true);
            return true;
        }

        public static void EnterPassword(out string pass) // проверяет на цифры и иниц. пароль
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите пароль цифрами от 0 до 9: ");
                pass = Console.ReadLine();
                if (Account.IsNumbers(pass) == false)
                    continue;
                break;
            }
            while (true);
        }

        public static void EnterBirthDay(out DateTime dateTime)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите Вашу дату рождения в формате дд.мм.гггг : ");
                try
                {
                    dateTime = Convert.ToDateTime(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Неправильный формат!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);
        }

        public void Register()
        {
            string login;
            do
            {
                Console.Clear();
                Console.WriteLine("Введите Логин латинскими символами верхнего и(или) нижнего регистра: ");
                login = Console.ReadLine();
                if (Account.IsEnglishLanguage(login) == false)
                    continue;
                if (Accs.ContainsKey(login) == true)
                {
                    Console.Clear();
                    Console.WriteLine("Данный логин уже зарегистрирован!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            string password;
            EnterPassword(out password);
            Accs.Add(login, password);

            DateTime dateTime;
            EnterBirthDay(out dateTime);
            SaveNewAccInFile(dateTime);

            MyAccount = new Account(login, password);

            Console.Clear();
            Console.WriteLine("Регистрация прошла успешно!");
            Console.ReadKey();
        }

        public void SaveNewAccInFile(DateTime dateTime)
        {
            using (StreamWriter writer = new StreamWriter(PathAccs, false, Encoding.Default))
            {
                foreach (var item in Accs)
                    writer.WriteLine($"{item.Key} - {item.Value} - {dateTime.ToString()}");
            }
        }

        public bool IsTruePassword(string login, string pass)
        {
            if (Accs[login] == pass)
                return true;
            else
            {
                Console.Clear();
                Console.WriteLine("Введен неверный пароль!");
                Console.ReadKey();
                return false;
            }
        }

        public static void SaveStatisticsInFile(Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            using (StreamWriter writer = new StreamWriter(PathStats, false, Encoding.Default))
            {
                foreach (var item in statistics)
                    writer.WriteLine($"\"{item.Key.Key}\" - {item.Key.Value} - {item.Value}");
            }
        }

        public void ViewTop20()
        {
            int choiceQuiz;
            string quizNameToView;
            do
            {
                Console.Clear();
                Console.WriteLine("Выберите викторину для просмотра ТОП-20 лучших прошедших ее пользователей: ");
                Console.WriteLine();
                List<int> quizIds = new List<int>();
                foreach (Quiz quiz in MyAllQuizzez.AllQuizzezList)
                {
                    Console.WriteLine($"{quiz.QuizId} - {quiz.QuizName}");
                    quizIds.Add(Convert.ToInt32(quiz.QuizId));
                }
                Console.WriteLine("\nДля просмотра ТОП-20 смешанных викторин введите слово \"микс\".");
                Console.Write("\nВвод: ");
                try
                {
                    string choice = Console.ReadLine();
                    if (choice == "микс")
                    {
                        quizNameToView = "Смешанная викторина";
                        break;
                    }
                    choiceQuiz = Convert.ToInt32(choice);
                }
                catch
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                if (quizIds.Contains(choiceQuiz) == false)
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                quizNameToView = MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizName;
                break;
            }
            while (true);

            var statsToShow = from x in Statistics
                              where x.Key.Key.Contains(quizNameToView)
                              orderby x.Value descending
                              select x;
            if (statsToShow == null || statsToShow.Count() == 0)
            {
                Console.Clear();
                Console.WriteLine($"Викторину \"{quizNameToView}\" еще никто не проходил!");
                Console.ReadKey();
            }
            else
            {
                var results = statsToShow.Take(20);
                Console.Clear();
                Console.WriteLine($"ТОП-20 участников викторины \"{quizNameToView}\":");
                Console.WriteLine();
                int i = 1;
                foreach (var item in results)
                    Console.WriteLine($"{i++} место: {item.Key.Value} ({item.Value} правильных ответов).");
                Console.ReadKey();
            }
        }

        public void ShowLoginOrRegister() // первый вызываемый метод меню.
        {
            char choice;
            do
            {
                Console.Clear();
                Console.WriteLine("Викторина v1.0");
                Console.WriteLine("\n1 - Регистрация нового пользователя");
                Console.WriteLine("2 - Вход в аккаунт");
                Console.WriteLine("\n3 - Выход из программы");
                choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        Register();
                        continue;
                    case '2':
                        string login;
                        if (EnterLogin(out login) == false)
                        {
                            choice = 'q';
                            continue;
                        }
                        else
                        {
                            string pass;
                            EnterPassword(out pass);
                            if (IsTruePassword(login, pass) == false)
                            {
                                choice = 'q';
                                continue;
                            }
                            MyAccount = new Account(login, pass);
                        }
                        break;
                    case '3':
                        break;
                    default:
                        continue;
                }
            }
            while (choice != '1' && choice != '2' && choice != '3');

            if (choice != '3')
                ShowMainMenu();
            else
                Console.Clear();
        }

        public void ShowMainMenu()
        {
            char choice;
            do
            {
                Console.Clear();
                Console.WriteLine($"Вход выполнен, {MyAccount.Login}!");
                Console.WriteLine("\n1 - Стартовать новую викторину");
                Console.WriteLine("2 - Посмотреть результаты своих прошлых викторин");
                Console.WriteLine("3 - Посмотреть Топ-20 по конкретной викторине");
                Console.WriteLine("\n4 - Поменять пароль");
                Console.WriteLine("5 - Изменить дату рождения");
                Console.WriteLine("\n6 - Выход из аккаунта");
                Console.WriteLine("7 - Выход из программы");
                choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        ShowMenuAllQuizzez();
                        continue;
                    case '2':
                        MyAccount.ViewPastQuizzezResults(Statistics);
                        continue;
                    case '3':
                        ViewTop20();
                        continue;
                    case '4':
                        MyAccount.ChangePassword();
                        continue;
                    case '5':
                        MyAccount.ChangeDateTimeBirthDay();
                        continue;
                    case '6':
                        MyAccount.Exit();
                        break;
                    case '7':
                        break;
                    default:
                        continue;
                }
            }
            while (choice != '7' && choice != '6');
            Console.Clear();
        }

        public void ShowMenuAllQuizzez()
        {
            int choiceQuiz;
            do
            {
                Console.Clear();
                Console.WriteLine("Выберите викторину для старта, нажав соответствующий ей номер: ");
                Console.WriteLine();
                List<int> quizIds = new List<int>();
                foreach (Quiz quiz in MyAllQuizzez.AllQuizzezList)
                {
                    Console.WriteLine($"{quiz.QuizId} - {quiz.QuizName}");
                    quizIds.Add(Convert.ToInt32(quiz.QuizId));
                }
                Console.WriteLine("\nДля старта викторины со случайными вопросами из всех викторин введите слово \"микс\".");
                Console.Write("\nВвод: ");
                try
                {
                    string choice = Console.ReadLine();
                    if (choice == "микс")
                    {
                        choiceQuiz = -1;
                        break;
                    }
                    choiceQuiz = Convert.ToInt32(choice);
                }
                catch
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                if (quizIds.Contains(choiceQuiz) == false)
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            if (choiceQuiz == -1)
                MyAccount.StartMixedQuiz(MyAllQuizzez, Statistics);
            else
                MyAccount.StartQuiz(choiceQuiz, MyAllQuizzez, Statistics);
        }
    }
    public class Account
    {
        public string Login;
        public string Password;
        public DateTime DateTimeBirthDay;

        public Account(string login, string password)
        {
            Login = login;
            Password = password;
            ReadBirthDayFromFile();
        }

        public void ReadBirthDayFromFile()
        {
            using (FileStream fileStream = new FileStream(Menu.PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexDateTime = new Regex(@"\d{2}.\d{2}.\d{4}");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains($"{Login}") == true)
                        {
                            Match matchBirth = regexDateTime.Match(currentLine);
                            DateTimeBirthDay = Convert.ToDateTime(matchBirth.ToString());
                            break;
                        }
                    }
                }
            }
        }

        public void ChangePassword()
        {
            string newPassword;
            Menu.EnterPassword(out newPassword);
            string oldPassword = Password;
            Password = newPassword;

            SaveAccauntDataInFile(oldPassword, DateTimeBirthDay);

            Console.Clear();
            Console.WriteLine("Пароль успешно обновлен!");
            Console.ReadKey();
        }

        public void ChangeDateTimeBirthDay()
        {
            DateTime newDateTimeBirthDay;
            Menu.EnterBirthDay(out newDateTimeBirthDay);
            DateTime oldBirthDay = DateTimeBirthDay;
            DateTimeBirthDay = newDateTimeBirthDay;

            SaveAccauntDataInFile(Password, oldBirthDay);

            Console.Clear();
            Console.WriteLine("Дата рождения успешно обновлена!");
            Console.ReadKey();
        }

        private void SaveAccauntDataInFile(string oldPassword, DateTime oldDateTime)
        {
            List<string> tmp = new List<string>();
            using (FileStream fileStream = new FileStream(Menu.PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        tmp.Add(currentLine);
                    }
                }
                int deleteIndex = Array.FindIndex(tmp.ToArray(), s => s == $"{Login} - {oldPassword} - {oldDateTime.ToString()}");
                tmp.RemoveAt(deleteIndex);
                tmp.Add($"{Login} - {Password} - {DateTimeBirthDay.ToString()}");
            }
            using (StreamWriter writer = new StreamWriter(Menu.PathAccs, false, Encoding.Default))
            {
                foreach (string item in tmp)
                    writer.WriteLine(item);
            }
        }

        public void StartQuiz(int choiceQuiz, AllQuizzez myAllQuizzez, Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            Console.Clear();
            Console.WriteLine($"Начинаем викторину \"{myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName}\".");
            Console.WriteLine("\nДля продолжения нажмите любую кнопку...");
            Console.ReadKey();

            int countRightQuestions = 0;
            int i = 0;
            while (i < myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList.Count)
            {
                Console.Clear();
                Console.WriteLine(myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].Question);
                Console.WriteLine();
                for (int j = 0; j < myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].PossibleOptionsList.Count; j++)
                {
                    Console.WriteLine($"{j} - {myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].PossibleOptionsList[j]}");
                }
                Console.WriteLine("\nВведите номер правильного ответа.");
                Console.WriteLine("Если их несколько, введите номера подряд без пробелов и запятых и нажмите Enter, иначе ответ не засчитается.");
                Console.Write("\nВвод: ");
                string userAnswers = Console.ReadLine();
                int countRightAnswers = 0;
                string tmp = "";
                foreach (char answer in userAnswers)
                {
                    if (userAnswers.Length != myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].AnswersList.Count)
                        break;

                    foreach (int answ in myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].AnswersList)
                        tmp += answ.ToString();

                    if (tmp.Contains(answer))
                        ++countRightAnswers;
                }
                if (countRightAnswers == myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].AnswersList.Count)
                    ++countRightQuestions;
                i++;
            }

            KeyValuePair<string, string> del = new KeyValuePair<string, string>(myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName, Login);
            statistics.Remove(del);
            statistics.Add(new KeyValuePair<string, string>(myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName, Login), countRightQuestions);
            Menu.SaveStatisticsInFile(statistics);

            Console.Clear();
            Console.WriteLine("Викторина окончена!");
            Console.WriteLine($"Количество правильных ответов: {countRightQuestions} из {myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList.Count}.");
            Console.WriteLine();

            ShowMyPlaceInCurrentQuiz(statistics, myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName);
        }

        public void StartMixedQuiz(AllQuizzez myAllQuizzez, Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            Console.Clear();
            Console.WriteLine($"Начинаем смешанную викторину!");
            Console.WriteLine("\nДля продолжения нажмите любую кнопку...");
            Console.ReadKey();

            List<XmlQuestionData> allQuestions = new List<XmlQuestionData>();
            foreach (Quiz quiz in myAllQuizzez.AllQuizzezList)
            {
                foreach (XmlQuestionData question in quiz.QuizList)
                    allQuestions.Add(question);
            }

            Random rnd = new Random();
            for (int l = 0; l < allQuestions.Count; l++)
            {
                int k = rnd.Next(0, l);
                XmlQuestionData value = allQuestions[k];
                allQuestions[k] = allQuestions[l];
                allQuestions[l] = value;
            }
            allQuestions.RemoveRange(20, allQuestions.Count - 20);

            int countRightQuestions = 0;
            int i = 0;
            while (i < allQuestions.Count)
            {
                Console.Clear();
                Console.WriteLine(allQuestions[i].Question);
                Console.WriteLine();

                for (int j = 0; j < allQuestions[i].PossibleOptionsList.Count; j++)
                    Console.WriteLine($"{j} - {allQuestions[i].PossibleOptionsList[j]}");

                Console.WriteLine("\nВведите номер правильного ответа.");
                Console.WriteLine("Если их несколько, введите номера подряд без пробелов и запятых и нажмите Enter, иначе ответ не засчитается.");
                Console.Write("\nВвод: ");
                string userAnswers = Console.ReadLine();
                int countRightAnswers = 0;
                string tmp = "";
                foreach (char answer in userAnswers)
                {
                    if (userAnswers.Length != allQuestions[i].AnswersList.Count)
                        break;

                    foreach (int answ in allQuestions[i].AnswersList)
                        tmp += answ.ToString();

                    if (tmp.Contains(answer))
                        ++countRightAnswers;
                }
                if (countRightAnswers == allQuestions[i].AnswersList.Count)
                    ++countRightQuestions;
                i++;
            }

            KeyValuePair<string, string> del = new KeyValuePair<string, string>("Смешанная викторина", Login);
            statistics.Remove(del);
            statistics.Add(new KeyValuePair<string, string>("Смешанная викторина", Login), countRightQuestions);
            Menu.SaveStatisticsInFile(statistics);

            Console.Clear();
            Console.WriteLine("Викторина окончена!");
            Console.WriteLine($"Количество правильных ответов: {countRightQuestions} из {allQuestions.Count}.");
            Console.WriteLine();

            ShowMyPlaceInCurrentQuiz(statistics, "Смешанная викторина");
        }

        public void ShowMyPlaceInCurrentQuiz(Dictionary<KeyValuePair<string, string>, int> statistics, string quizName)
        {
            var currentQuizStats = from x in statistics
                                   where x.Key.Key.Contains(quizName)
                                   orderby x.Value descending
                                   select x;
            int indexMyPlace = currentQuizStats.ToList().FindIndex(x => x.Key.Value == Login);
            Console.WriteLine($"Ваше место среди всех участников данной викторины: {++indexMyPlace} из {currentQuizStats.Count()}");
            Console.ReadKey();
        }

        public void ViewPastQuizzezResults(Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            Console.Clear();
            Console.WriteLine("Результаты ваших ранее пройденных викторин:");
            Console.WriteLine();
            var MyQuizzezStats = from x in statistics
                                 where x.Key.Value.Contains(Login)
                                 select x;
            if (MyQuizzezStats == null || MyQuizzezStats.Count() == 0)
            {
                Console.Clear();
                Console.WriteLine($"Вы еще не прошли ни одной викторины!");
                Console.ReadKey();
            }
            else
            {
                foreach (var item in MyQuizzezStats)
                    Console.WriteLine($"\"{item.Key.Key}\" - {item.Value} правильных ответов.");
                Console.ReadKey();
            }
        }

        public void Exit()
        {
            Menu newMenu = new Menu();
        }

        public static Language CheckSymbolLanguage(int symbol)
        {
            if ((symbol >= 65 && symbol <= 90) || symbol >= 97 && symbol <= 122)
                return Language.English;
            if (symbol >= 48 && symbol <= 57)
                return Language.Numbers;
            return Language.Unknown;
        }

        public static bool IsEnglishLanguage(string word)
        {
            Language current = Language.English;
            Language tmp;
            for (int i = 0; i < word.Length; i++)
            {
                tmp = CheckSymbolLanguage(word[i]);
                if (current != tmp)
                {
                    Console.WriteLine("Допустимый ввод логина - латинские символы верхнего и нижнего регистра!");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }

        public static bool IsNumbers(string password)
        {
            Language current = Language.Numbers;
            Language tmp;
            for (int i = 0; i < password.Length; i++)
            {
                tmp = CheckSymbolLanguage(password[i]);
                if (current != tmp)
                {
                    Console.WriteLine("Допустимый ввод пароля - цифры от 0 до 9!");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }
    }
    public class AllQuizzez
    {
        public static string Path;
        public List<Quiz> AllQuizzezList;

        static AllQuizzez()
        {
            Path = "quizes.xml";
        }

        public AllQuizzez()
        {
            AllQuizzezList = new List<Quiz>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Path);
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode quiz in xRoot)
            {
                XmlNode quizAttr = quiz.Attributes.GetNamedItem("id");
                int quizAttrId = Convert.ToInt32(quizAttr.Value); // получили id викторины
                quizAttr = quiz.Attributes.GetNamedItem("name");
                string quizAttrName = quizAttr.Value; // получили название викторины

                List<XmlQuestionData> questionList = new List<XmlQuestionData>();

                foreach (XmlNode item in quiz)
                {
                    XmlNode itemAttr = item.Attributes.GetNamedItem("id");
                    int itemAttrId = Convert.ToInt32(itemAttr.Value); // получили id вопроса   

                    string question = null;
                    List<string> options = new List<string>();
                    List<int> answers = new List<int>();

                    foreach (XmlNode childItem in item.ChildNodes)
                    {

                        if (childItem.Name == "question")
                            question = childItem.InnerText; // получили вопрос

                        if (childItem.Name == "possible")
                            options.Add(childItem.InnerText); // получили вариант ответа и добавили в массив

                        if (childItem.Name == "answer")
                            answers.Add(Convert.ToInt32(childItem.InnerText)); // получили номер правильного ответа и добавили в массив     
                    }

                    XmlQuestionData currentQuestion = new XmlQuestionData(itemAttrId, question, options, answers);
                    questionList.Add(currentQuestion);
                }

                Quiz currentQuiz = new Quiz(quizAttrId, quizAttrName, questionList);
                AllQuizzezList.Add(currentQuiz); // добавили все викторины со всем содержимым в массив
            }
        }
    }
    public class Quiz
    {
        public int QuizId;
        public string QuizName;
        public List<XmlQuestionData> QuizList;

        public Quiz(int quizId, string quizName, List<XmlQuestionData> quizList)
        {
            QuizId = quizId;
            QuizName = quizName;
            QuizList = new List<XmlQuestionData>(quizList);
        }
    }
    public class XmlQuestionData
    {
        public int ItemId;
        public string Question;
        public List<string> PossibleOptionsList;
        public List<int> AnswersList;

        public XmlQuestionData(int itemId, string question, List<string> possibleOptions, List<int> answers)
        {
            ItemId = itemId;
            Question = question;
            PossibleOptionsList = new List<string>(possibleOptions);
            AnswersList = new List<int>(answers);
        }
    }
}