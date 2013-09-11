using System; 
using System.Collections.Specialized; 
using System.Configuration; 
using System.Text; 
using System.Xml; 
namespace Croc.Core.Utils.Text 
{ 
    public class RusNumber 
    { 
        private static string[] hunds = 
        { 
            "", "сто ", "двести ", "триста ", "четыреста ", 
            "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот " 
        }; 
        private static string[] tens = 
        { 
            "", "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ", 
            "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто " 
        }; 
        public static string Str(int val, bool male, string one, string two, string five) 
        { 
            string[] frac20 = 
            { 
                "", "один ", "два ", "три ", "четыре ", "пять ", "шесть ", 
                "семь ", "восемь ", "девять ", "десять ", "одиннадцать ", 
                "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ", 
                "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать " 
            }; 
            int num = val % 1000; 
            if (0 == num) return "ноль"; 
            if (num < 0) throw new ArgumentOutOfRangeException("val", "Параметр не может быть отрицательным"); 
            if (!male) 
            { 
                frac20[1] = "одна "; 
                frac20[2] = "две "; 
            } 
            StringBuilder r = new StringBuilder(hunds[num / 100]); 
            if (num % 100 < 20) 
            { 
                r.Append(frac20[num % 100]); 
            } 
            else 
            { 
                r.Append(tens[num % 100 / 10]); 
                r.Append(frac20[num % 10]); 
            } 
            r.Append(Case(num, one, two, five)); 
            if (r.Length != 0) r.Append(" "); 
            return r.ToString(); 
        } 
        public static string Case(int val, string one, string two, string five) 
        { 
            int t = (val % 100 > 20) ? val % 10 : val % 20; 
            switch (t) 
            { 
                case 1: return one; 
                case 2: 
                case 3: 
                case 4: return two; 
                default: return five; 
            } 
        } 
    } 
    public struct CurrencyInfo 
    { 
        public bool male; 
        public string seniorOne; 
        public string seniorTwo; 
        public string seniorFive; 
        public string juniorOne; 
        public string juniorTwo; 
        public string juniorFive; 
    } 
    public class RusCurrencySectionHandler : IConfigurationSectionHandler 
    { 
        public object Create(object parent, object configContext, XmlNode section) 
        { 
            foreach (XmlNode curr in section.ChildNodes) 
            { 
                if (curr.Name == "currency") 
                { 
                    XmlNode senior = curr["senior"]; 
                    XmlNode junior = curr["junior"]; 
                    RusCurrency.Register( 
                        curr.Attributes["code"].InnerText, 
                        (curr.Attributes["male"].InnerText == "1"), 
                        senior.Attributes["one"].InnerText, 
                        senior.Attributes["two"].InnerText, 
                        senior.Attributes["five"].InnerText, 
                        junior.Attributes["one"].InnerText, 
                        junior.Attributes["two"].InnerText, 
                        junior.Attributes["five"].InnerText); 
                } 
            } 
            return null; 
        } 
    }; 
    public class RusCurrency 
    { 
        private static readonly HybridDictionary currencies = new HybridDictionary(); 
        static RusCurrency() 
        { 
            Register("RUR", true, "рубль", "рубля", "рублей", "копейка", "копейки", "копеек"); 
            Register("EUR", true, "евро", "евро", "евро", "евроцент", "евроцента", "евроцентов"); 
            Register("USD", true, "доллар", "доллара", "долларов", "цент", "цента", "центов"); 
        } 
        public static void Register(string currency, bool male, 
            string seniorOne, string seniorTwo, string seniorFive, 
            string juniorOne, string juniorTwo, string juniorFive) 
        { 
            CurrencyInfo info; 
            info.male = male; 
            info.seniorOne = seniorOne; info.seniorTwo = seniorTwo; info.seniorFive = seniorFive; 
            info.juniorOne = juniorOne; info.juniorTwo = juniorTwo; info.juniorFive = juniorFive; 
            currencies.Add(currency, info); 
        } 
        public static string Str(double val) 
        { 
            return Str(val, "RUR"); 
        } 
        public static string Str(double val, string currency) 
        { 
            if (!currencies.Contains(currency)) 
                throw new ArgumentOutOfRangeException("currency", "Валюта \"" + currency + "\" не зарегистрирована"); 
            CurrencyInfo info = (CurrencyInfo)currencies[currency]; 
            return Str(val, info.male, 
                info.seniorOne, info.seniorTwo, info.seniorFive, 
                info.juniorOne, info.juniorTwo, info.juniorFive); 
        } 
        public static string Str(double val, bool male, 
            string seniorOne, string seniorTwo, string seniorFive, 
            string juniorOne, string juniorTwo, string juniorFive) 
        { 
            bool minus = false; 
            if (val < 0) { val = -val; minus = true; } 
            int n = (int)val; 
            int remainder = (int)((val - n + 0.005) * 100); 
            StringBuilder r = new StringBuilder(); 
            if (0 == n) r.Append("0 "); 
            if (n % 1000 != 0) 
                r.Append(RusNumber.Str(n, male, seniorOne, seniorTwo, seniorFive)); 
            else 
                r.Append(seniorFive); 
            n /= 1000; 
            r.Insert(0, RusNumber.Str(n, false, "тысяча", "тысячи", "тысяч")); 
            n /= 1000; 
            r.Insert(0, RusNumber.Str(n, true, "миллион", "миллиона", "миллионов")); 
            n /= 1000; 
            r.Insert(0, RusNumber.Str(n, true, "миллиард", "миллиарда", "миллиардов")); 
            n /= 1000; 
            r.Insert(0, RusNumber.Str(n, true, "триллион", "триллиона", "триллионов")); 
            n /= 1000; 
            r.Insert(0, RusNumber.Str(n, true, "триллиард", "триллиарда", "триллиардов")); 
            if (minus) r.Insert(0, "минус "); 
            r.Append(remainder.ToString("00 ")); 
            r.Append(RusNumber.Case(remainder, juniorOne, juniorTwo, juniorFive)); 
            r[0] = char.ToUpper(r[0]); 
            return r.ToString(); 
        } 
    }; 
    public class CustomRusNumber 
    { 
        public static string Str(double val, bool male) 
        { 
            bool minus = false; 
            if (val < 0) { val = -val; minus = true; } 
            int n = (int)val; 
            int remainder = (int)((val - n + 0.005) * 100); 
            StringBuilder r = new StringBuilder(); 
            if (n == 0) 
                return "ноль"; 
            if (n % 1000 != 0) 
                r.Append(RusNumber.Str(n, male, "", "", "")); 
            n /= 1000; 
            int nCur = n % 1000;    // значение в исследуемом разряде 
            if (nCur > 0 && nCur < 1000) 
                r.Insert(0, RusNumber.Str(nCur, false, "тысяча", "тысячи", "тысяч")); 
            n /= 1000; 
            nCur = n % 1000; 
            if (nCur > 0 && nCur < 1000) 
                r.Insert(0, RusNumber.Str(nCur, true, "миллион", "миллиона", "миллионов")); 
            n /= 1000; 
            nCur = n % 1000; 
            if (nCur > 0 && nCur < 1000) 
                r.Insert(0, RusNumber.Str(nCur, true, "миллиард", "миллиарда", "миллиардов")); 
            n /= 1000; 
            nCur = n % 1000; 
            if (nCur > 0 && nCur < 1000) 
                r.Insert(0, RusNumber.Str(nCur, true, "триллион", "триллиона", "триллионов")); 
            n /= 1000; 
            nCur = n % 1000; 
            if (nCur > 0 && nCur < 1000) 
                r.Insert(0, RusNumber.Str(nCur, true, "квадриллион", "квадриллиона", "квадриллионов")); 
            if (minus) r.Insert(0, "минус "); 
            return r.ToString(); 
        } 
    } 
}
