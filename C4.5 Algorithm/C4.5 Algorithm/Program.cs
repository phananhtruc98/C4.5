using NumSharp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace C4._5_Algorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] data = ReadFile("E:\\C4.5\\C4.5 Algorithm\\C4.5 Algorithm\\data.txt");
            string[] df = data;
            double entropy = CalculateEntropy(df);
            FindDecision(df);
            Console.WriteLine(entropy);
            Console.ReadKey();
        }
        static string[] CreateNewSubDf(string[] S, string collumn_name)
        {
            int index = S[0].Split(',').ToList().FindIndex(x => x == collumn_name);
            Console.WriteLine("Index: "+ index);
            for(int i = 0;i <= S.Length - 1; i++)
            {
                S[i] = String.Join(",", S[i].Split(',').RemoveAt(index).ToArray());
            }
            return S;
        }
        static string FindDecision(string[] df)
        {
            double entropy = CalculateEntropy(df);
            int rows = df.Length;
            int columns = df[0].Split(',').Length;
            List<double> gain_ratios = new List<double>();
            // for các cột trừ cột quyết định
            for (int i = 0; i <= columns - 1; i++)
            {
                string column_name = df[0].Split(',')[i];
                string column_type = df[1].Split(',')[i];
                // nếu là int thì tính continuous data
                if (int.TryParse(column_type, out int output))
                {
                    Console.WriteLine(column_name + " is int");
                    // Gọi hàm tính df mới
                    //	#Convert numeric column to nominal column
                    //	if column_type != 'object':
                    //		df = Preprocess.processContinuousFeatures(df, column_name, entropy)
                }
                List<string> classes = df.Select(x => x.Split(',')[i]).Distinct().ToList();
                int classes_count = classes.Count();
                double gain = entropy * 1;
                Console.WriteLine(classes);
                for(int j = 0;j <= classes_count; j++)
                {
                    string current_class = classes[j];
                    // viết 1 hàm remove cột có class=abc
                    string[] sub_df = CreateNewSubDf(df, current_class);
                    int sub_df_rows = sub_df.Length;
                    double p = double.Parse(sub_df_rows.ToString()) / double.Parse(rows.ToString());
                    double sub_df_entropy = CalculateEntropy(sub_df);
                    gain = gain - p * sub_df_entropy;
                    double split_info = -(p * Math.Log(p, 2));
                    double gain_ratio = gain / split_info;
                    gain_ratios.Add(gain_ratio);
                }
            }

            int winner_index = gain_ratios.IndexOf(gain_ratios.Max());
            Console.WriteLine(winner_index);

            return "hello"; 

 //       classes = df[column_name].value_counts() # số lớp của cột
	//	gain = entropy * 1; split_info = 0


 //       for j in range(0, len(classes)):

 //           current_class = classes.keys().tolist()[j]

 //           sub_df = df[df[column_name] == current_class]

 //           sub_df_row = sub_df.shape[0]

 //           p = sub_df_row / rows

 //           sub_df_entropy = calcEntropy(sub_df)

 //           gain = gain - p * sub_df_entropy

 //           split_info = split_info - p * math.log(p, 2)


 //       if split_info == 0:
	//		split_info = 100

 //       gain_ratio = gain / split_info

 //       gain_ratios.append(gain_ratio)


 //   winner_index = gain_ratios.index(max(gain_ratios))

 //   winner_name = df.columns[winner_index]


 //   return winner_name
        }
        static double CalculateEntropy(string[] df)
        {
            int rows = df.Length;
            int cols = df[0].Split(',').Length;
            List<string> decision_values = df.Select(x => x.Split(',')[cols - 1]).Distinct().ToList();
            List<string> decisions = df.Select(x => x.Split(',')[cols - 1]).ToList();
            double entropy = 0;
            foreach(string decision_value in decision_values)
            {
                int amount_same_decision = decisions.Where(x => x == decision_value).Count();
                double p = double.Parse(amount_same_decision.ToString()) / double.Parse(rows.ToString());
                entropy = entropy - p*Math.Log(p, 2);
            }            
            return entropy;
        }
        static string[] ReadFile(string path)
        {
            return File.ReadAllLines(path);
        }
        static string[,] ConvertFileTo2dArray (string path)
        {
            string[] one_d = File.ReadAllLines(path);
        int cols = one_d[1].Split(',').Length;
        int rows = one_d.Length;
        string[,] df = new string[rows, cols];
            for (int i=0; i<one_d.Length; i++)
            {
                string[] row = one_d[i].Split(',');
                for(int j = 0; j<row.Length; j++)
                {
                    df[i, j] = row[j];
                }
}
            return df;
        }
    }
}
