using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Exercise1.Controller;
using Exercise1.Model;

namespace Exercise1
{
    class Program
    {
        private static RecommenderSystem m_recommenderSystem;
        static void Main(string[] args)
        {
            string resultModel;
            do
            {
                Console.WriteLine("Which model would you like to execute? (1 = CF , 2 = Content Model)");
                resultModel = Console.ReadLine();
                if (resultModel != "1" && resultModel != "2")
                    Console.WriteLine("Please choose '1' or '2' only");
            } while (resultModel != "1" && resultModel != "2");

            string latentFeatures;
            int latentFeatureNumber = 0;
            bool validNumber = true;
            do
            {
                Console.WriteLine("How Many latent features?");
                latentFeatures = Console.ReadLine();
                try
                {
                    latentFeatureNumber = Convert.ToInt32(latentFeatures);
                    if (latentFeatureNumber <= 0)
                    {
                        validNumber = false;
                        Console.WriteLine("Please Enter positive number.");
                    }
                }
                catch (Exception)
                {
                    validNumber = false;
                    Console.WriteLine("Insert positive number");
                }
            } while (validNumber == false);

            
            m_recommenderSystem = new RecommenderSystem(latentFeatureNumber,resultModel == "1"?true:false);
        }
    }
}
