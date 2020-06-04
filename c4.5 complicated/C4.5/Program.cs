using NumSharp.Extensions;
using NumSharp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace C4._5
{
    class Program
    {

        static void Main(string[] args)
        {
            string[] base_data = ReadFile("E:\\C4.5\\C4.5\\data.txt");
            string[] dataset = base_data.RemoveAt(0);
            string nodes = base_data.FirstOrDefault();
            double Entropy_of_Dataset = EntropyMain(dataset);
            string[] temp = ConvertContinuous(dataset, GetIndexCollumnContinuousData(dataset), Entropy_of_Dataset);
            temp.ToList().ForEach(x => Console.WriteLine(x));
            //double rs = GetMaxGainRatioInContinuous(dataset, 2, Entropy_of_Dataset).Value;
            //Console.WriteLine(rs);
            int rs = MakeDecision(temp, Entropy_of_Dataset).Key;
            Console.WriteLine("Root node: " + nodes.Split(',')[rs]);
            GetSecondNode(temp, rs);
            Console.ReadLine();
        }

        /// <summary>
        ///  Calculate Entropy of dataset
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        static double EntropyMain(string[] S)
        {

            List<string> decision = new List<string>();
            for (int i = 0; i <= S.Length - 1; i++)
            {
                string[] words = S[i].Split(',');
                decision.Add(words.Last());
            }
            List<double> entropies = new List<double>();
            List<string> distinct_value = decision.Select(x => x).Distinct().ToList();
            foreach (string value in distinct_value)
            {
                double entropy = -(double.Parse(decision.Where(x => x == value).Count().ToString()) / double.Parse(S.Length.ToString())
                    * Math.Log(double.Parse(decision.Where(x => x == value).Count().ToString()) / double.Parse(S.Length.ToString()), 2));
                entropies.Add(entropy);
            }

            return entropies.Sum(x => x);
        }


        /// <summary>
        /// GetIndexCollumnContinuousData
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        static List<int> GetIndexCollumnContinuousData(string[] S)
        {
            List<int> index_collumns = new List<int>();
            for (int i = 0; i < S.Length - 1; i++)
            {
                for (int j = 0; j < S[0].Split(',').Length - 1; j++)
                {
                    if (int.TryParse(S[0].Split(',')[j], out int output))
                    {
                        index_collumns.Add(j);
                    }
                }
            }
            return index_collumns.Distinct().ToList();
        }

        /// <summary>
        /// GetMaxGainRatio
        /// </summary>
        /// <param name="value_decision_list"></param>
        /// <returns></returns>
        static IntDouble GetMaxGainRatioInContinuous(string[] S, int index_collumn, double entropy_main)
        {
            List<int> values = new List<int>();
            List<IntString> new_data = new List<IntString>();
            for (int i = 0; i <= S.Length - 1; i++)
            {
                string[] words = S[i].Split(',');
                values.Add(int.Parse(words[index_collumn]));
                new_data.Add(new IntString(int.Parse(words[index_collumn]), words.Last()));
            }
            values = values.Distinct().OrderBy(x=>x).ToList();
            List<IntDouble> gain_ratio_each_value = new List<IntDouble>();
            foreach (int value in values.Distinct().ToList())
            {
                double gain_ratio = GetGainRatio(entropy_main, new_data, value);
                gain_ratio_each_value.Add(new IntDouble(value, gain_ratio));
            }
            
            double value_max = gain_ratio_each_value.Max(x => x.Value);

            IntDouble rs = gain_ratio_each_value.Where(x => x.Value == value_max).FirstOrDefault();
            return rs;
        }

        private static double GetGain(double entropy_main, List<IntString> dataset, int main_value)
        {
            double dataset_length = double.Parse(dataset.Count.ToString());
            List<StringString> lower_main = new List<StringString>();
            List<StringString> higher_main = new List<StringString>();
            foreach (IntString row in dataset)
            {
                if (row.Key <= main_value) lower_main.Add(new StringString(row.Key.ToString(), row.Value));
                else higher_main.Add(new StringString(row.Key.ToString(), row.Value));
            }
            double entropy_lower = Entropy(lower_main);
            double entropy_higher = Entropy(higher_main);
            double gain = entropy_main - ((double.Parse(lower_main.Count.ToString()) / dataset_length) * entropy_lower + (double.Parse(higher_main.Count.ToString()) / dataset_length) * entropy_higher);
            return gain;
        }

        private static double GetSplitInfo(double entropy_main, List<IntString> dataset, int main_value)
        {
            double dataset_length = double.Parse(dataset.Count.ToString());
            List<StringString> lower_main = new List<StringString>();
            List<StringString> higher_main = new List<StringString>();
            foreach (IntString row in dataset)
            {
                if (row.Key <= main_value) lower_main.Add(new StringString(row.Key.ToString(), row.Value));
                else higher_main.Add(new StringString(row.Key.ToString(), row.Value));
            }

            double lower_length = double.Parse(lower_main.Count.ToString());
            double higher_length = double.Parse(higher_main.Count.ToString());

            return -((lower_length / dataset_length) * Math.Log((lower_length / dataset_length), 2) + (higher_length / dataset_length) * Math.Log((higher_length / dataset_length), 2));
        }

        private static double GetGainRatio(double entropy_main, List<IntString> dataset, int main_value)
        {
            double gain = GetGain(entropy_main, dataset, main_value);
            double split_info = Double.IsNaN(GetSplitInfo(entropy_main, dataset, main_value)) ? 0 : GetSplitInfo(entropy_main, dataset, main_value);
            return Double.IsNaN(gain / split_info) ? 0: gain / split_info;
        }


        static double Entropy(List<StringString> value_decision_list)
        {
            double total_length = value_decision_list.Count;
            List<double> entropies = new List<double>();
            List<string> distinct_value = value_decision_list.Select(x => x.Value).Distinct().ToList();
            List<StringInt> pairs = new List<StringInt>();
            foreach (string value in distinct_value)
            {
                pairs.Add(new StringInt(value, value_decision_list.Where(x => x.Value == value).Count()));
            }
            double rs = 0;
            foreach(StringInt item in pairs)
            {
                rs -= (double.Parse(item.Value.ToString()) / total_length) * Math.Log((double.Parse(item.Value.ToString()) / total_length), 2);
            }
            return rs;
        }

        static string[] ConvertContinuous(string[] S, List<int> index_collumns, double entropy_main)
        {
            List<string> S_new = new List<string>();
            foreach(int index in index_collumns)
            {
                IntDouble final = GetMaxGainRatioInContinuous(S, index, entropy_main);
                List<int> values = new List<int>();
                List<IntString> new_data = new List<IntString>();
                S_new = new List<string>();
                for (int i = 0; i <= S.Length - 1; i++)
                {
                    
                    string[] words = S[i].Split(',');
                    if (int.Parse(words[index]) <= final.Key) words[index] = "<=" + final.Key.ToString();
                    else words[index] = ">" + final.Key.ToString();
                    S_new.Add(String.Join(",", words));
                }
                S = S_new.ToArray();
            }
            return S_new.ToArray();
        }

        private static double GetGainRatioMain(double entropy_main, List<StringString> dataset)
        {
            double gain = GetGainMain(entropy_main, dataset);
            double split_info = Double.IsNaN(GetSplitInfoMain(dataset)) ? 0 : GetSplitInfoMain(dataset);
            return Double.IsNaN(gain / split_info) ? 0 : gain / split_info;
        }

        private static double GetSplitInfoMain(List<StringString> dataset)
        {
            double plog = 0;
            List<string> distinct_value = dataset.Select(x => x.Key).Distinct().ToList();
            foreach (string value in distinct_value)
            {
                List<StringString> new_data = new List<StringString>();
                new_data = dataset.Where(x => x.Key == value).ToList();
                double fraction = (double.Parse(new_data.Count.ToString()) / double.Parse(dataset.Count.ToString()));
                plog += (fraction * Math.Log(fraction,2));
            }
            return -plog;
        }

        private static double GetGainMain(double entropy_main, List<StringString> dataset)
        {
            double entropy_sum = 0;
            List<string> distinct_value = dataset.Select(x => x.Key).Distinct().ToList();
            foreach(string value in distinct_value)
            {
                List<StringString> new_data = new List<StringString>();
                new_data = dataset.Where(x => x.Key == value).ToList();
                double fraction = (double.Parse(new_data.Count.ToString()) / double.Parse(dataset.Count.ToString()));
                entropy_sum += (Entropy(new_data)* fraction);
            }         
            return entropy_main - entropy_sum;
        }

        private static IntDouble MakeDecision(string[] S, double entropy_main)
        {
            List<IntDouble> listGainRatio = new List<IntDouble>();
            for (int i = 0; i <= S[0].Split(',').Length - 2; i++)
            {
                List<StringString> dataset = new List<StringString>();
                for (int j = 0; j <= S.Length - 1; j++)
                {
                    string[] words = S[j].Split(',');
                    StringString obj = new StringString(words[i],S[j].Split(',')[words.Length-1]);
                    dataset.Add(obj);
                }
                IntDouble gain_ratio = new IntDouble(i, GetGainRatioMain(entropy_main, dataset));
                listGainRatio.Add(gain_ratio);
            }
            double max_gain_ratio = listGainRatio.Max((x => x.Value));
            return listGainRatio.Where(x=>x.Value == max_gain_ratio).FirstOrDefault();
        }

        private static void GetSecondNode(string[] S, int RootNodeIndex)
        {
            List<string> values = GetValuesInProperty(S, RootNodeIndex);
            for(int i = 0; i <= values.Count - 1; i++)
            {
                Console.WriteLine(values[i]);
            }
        }




        private static List<string> GetValuesInProperty(string[] S, int index)
        {
            List<string> values = new List<string>();
            for (int i = 0; i <= S.Length - 1; i++)
            {
                values.Add(S[i].Split(',')[index]);
            }
            return values.Distinct().ToList();
        }


        static Dictionary<string, int> Entropy(string[] S, int collum_index = 1000)
        {
            List<string> decision = new List<string>();
            Dictionary<string, string> data = new Dictionary<string, string>();
            List<string> property_values = new List<string>();
            for (int i = 0; i <= S.Length - 1; i++)
            {
                if (collum_index == 1000)
                {
                    string[] words = S[i].Split(',');
                    decision.Add(words.Last());
                }
                else
                {
                    string[] words = S[i].Split(',');
                    property_values.Add(words[collum_index]);
                    data.Add(words[collum_index], words.Last());
                }
            }
            if (data != null)
            {
                Dictionary<string, string> dataset = new Dictionary<string, string>();
                List<string> distinct_value_property = property_values.Select(x => x).Distinct().ToList();
                foreach (string value in distinct_value_property)
                {
                    foreach (KeyValuePair<string, string> keyValue in data)
                    {
                        if (keyValue.Key == value) dataset.Add(value, keyValue.Value);
                    }

                }
            }
            decision.RemoveAt(0);
            List<string> distinct_value = decision.Select(x => x).Distinct().ToList();
            Console.WriteLine("Distinct values: ");
            Dictionary<string, int> pairs = new Dictionary<string, int>();
            foreach (string value in distinct_value)
            {
                pairs.Add(value, decision.Where(x => x == value).Count());
            }
            foreach (KeyValuePair<string, int> value in pairs)
            {
                Console.WriteLine("key: " + value.Key + ", value: " + value.Value);
            }
            return pairs;
        }

        void Entropy(Dictionary<string, int> keyValuePairs, int total)
        {
            double result = 0;
            foreach (KeyValuePair<string, int> value in keyValuePairs)
            {
                result += (value.Value / total) * Math.Log(value.Value / total);
            }
        }

        static string[] ReadFile(string path)
        {
            return File.ReadAllLines(path);
        }
       
    }
    class IntString{

        public IntString(int key, string value)
        {
            Key = key;
            Value = value;
        }

        public int Key { get; set; }
        public string Value { get; set; }
    }

    class IntDouble
    {
        public IntDouble(int key, double value)
        {
            Key = key;
            Value = value;
        }

        public int Key { get; set; }
        public double Value { get; set; }
    }

    class StringString
    {
        public StringString(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    class StringInt
    {
        public StringInt(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public int Value { get; set; }
    }

    class StringDouble
    {
        public StringDouble(string key, double value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public double Value { get; set; }
    }
}
