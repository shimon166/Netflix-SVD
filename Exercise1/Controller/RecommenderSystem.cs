using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Exercise1.Model;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace Exercise1.Controller
{
    public class RecommenderSystem
    {
        private DataManager m_datamanager;

        public RecommenderSystem(int latentFeatures, bool cfModel)
        {
            m_datamanager = new DataManager();
            if (cfModel)
            {
                m_datamanager.Load(true);
                Matrix<double> pMatrix, qMatrix;
                Vector<double> bu, bi;
                double miu;
                double minRMSE = TrainBaseModel(latentFeatures, out pMatrix, out qMatrix, out bu, out bi, out miu);
                double testRMSE = PredictRatingOnTestSet(pMatrix, qMatrix, bu, bi, miu);
                double MAE = this.MAE(pMatrix, qMatrix, m_datamanager.GetAllTestSetCombinedData(), bu, bi, miu);
                Console.WriteLine("Min RMSE on train set: "+minRMSE);
                Console.WriteLine("Min RMSE on test set: "+testRMSE);
                Console.WriteLine("Min MAE on test set: "+ MAE);
            }
            else
            {
                m_datamanager.Load(false);
                TrainContentModel(latentFeatures);    
            }
            
            }

        private double PredictRatingOnTestSet(Matrix<double> pMatrix, Matrix<double> qMatrix,Vector<double> bu,Vector<double> bi,double miu  )
        {
            var allTestsetData = m_datamanager.GetAllTestSetCombinedData();
            return RMSE(pMatrix, qMatrix, allTestsetData,bu,bi,miu);
        }

        private double PredictRating(Vector<double> vec1, Vector<double> vec2,double bu,double bi,double miu)
        {
            double result = miu+bu+bi+vec1.DotProduct(vec2);
            if (result > 5)
                return 5;
            if (result < 1)
                return 1;
            return result;
        }

    /// <summary>
        /// 
        /// </summary>
        /// <param name="latentFeatureNumber">p and q vector size</param>
        public double TrainBaseModel(int latentFeatureNumber, out Matrix<double> pMatrix, out Matrix<double> qMatrix, out Vector<double> bu,out Vector<double> bi,out double miu)
        {
            var allCombinedData = m_datamanager.GetAllTrainCombinedData();
            var allCombinedItems = m_datamanager.GetAllTrainCombinedItems();
            int numberofUsers = allCombinedData.Keys.Count;
            int numberOfItems = allCombinedItems.Keys.Count;
            Dictionary<User, List<Rating>> trainDataByUser;
            Dictionary<User, List<Rating>> validationDataByUser;
            Dictionary<int, User> allUsersByIndex;
            Dictionary<int, Item> allItemsByIndex;
            SplitDataToTrainAndValidation(0.8,allCombinedData,allCombinedItems,out trainDataByUser,out validationDataByUser,out allUsersByIndex,out allItemsByIndex);

            var traincount = trainDataByUser.Values.Sum(x => x.Count);
            var validationcount = validationDataByUser.Values.Sum(x => x.Count);
            Console.WriteLine("train count = " + traincount+", validation count = "+validationcount);
            Console.WriteLine(((double)traincount)/(traincount+validationcount));

            pMatrix = Matrix<double>.Build.Random(numberofUsers, latentFeatureNumber,new ContinuousUniform(0.1,0.1));
            qMatrix = Matrix<double>.Build.Random(latentFeatureNumber, numberOfItems, new ContinuousUniform(0.1,0.1));

            miu = trainDataByUser.SelectMany(x => x.Value).Average(a => a.RatingNumber);
            double learningRate = 0.001;
            double regularization = 0.01;
            bu = Vector<double>.Build.Random(allUsersByIndex.Count, new ContinuousUniform(0.01, 0.01));
            bi = Vector<double>.Build.Random(allItemsByIndex.Count, new ContinuousUniform(0.01, 0.01));
            double prevRMSE = RMSE(pMatrix, qMatrix, validationDataByUser,bu,bi,miu);
            double currentRMSE = prevRMSE;
            int ccc = 0;
            int dd = 0;

            
                for (int i = 0; i < latentFeatureNumber; i++)
                {
                    ccc = 0;
                    for (int epoch = 0; epoch < 120; epoch++)
                    {
                        prevRMSE = currentRMSE;
                        foreach (var user in trainDataByUser)
                        {
                            foreach (var rating in user.Value)
                            {
                                if (rating.RatedItem.MatrixIndex != -1)
                                {
                                    double err_ui = rating.RatingNumber -
                                                    PredictRating(pMatrix.Row(user.Key.MatrixIndex),
                                                        qMatrix.Column(rating.RatedItem.MatrixIndex),bu[user.Key.MatrixIndex],bi[rating.RatedItem.MatrixIndex],miu);
                                                    
                                    bu[user.Key.MatrixIndex] += learningRate*(err_ui - regularization*bu[user.Key.MatrixIndex]);
                                    bi[rating.RatedItem.MatrixIndex] += learningRate * (err_ui - regularization * bi[rating.RatedItem.MatrixIndex]);
                                    pMatrix[user.Key.MatrixIndex, i] += learningRate*
                                                                        (err_ui*
                                                                         qMatrix[i, rating.RatedItem.MatrixIndex
                                                                             ] -
                                                                         regularization*
                                                                         pMatrix[user.Key.MatrixIndex, i]);
                                    qMatrix[i, rating.RatedItem.MatrixIndex] += learningRate*
                                                                                (err_ui*
                                                                                 pMatrix[user.Key.MatrixIndex, i
                                                                                     ] -
                                                                                 regularization*
                                                                                 qMatrix[
                                                                                     i,
                                                                                     rating.RatedItem.MatrixIndex]);
                                }
                            }
                        }
                        currentRMSE = RMSE(pMatrix, qMatrix, validationDataByUser,bu,bi,miu);
                        if (currentRMSE > prevRMSE)
                        {
                            //Console.WriteLine("Feature number: "+i+"    Number of iterations: " +ccc);
                            break;
                        }
                            
                        ccc++;
                    }
                    //Console.WriteLine("Feature number: " + i + "    Number of iterations: " + ccc);
                
            }

            Console.WriteLine("Number of iterations: "+ccc);
            return prevRMSE;
        }



        private double MAE(Matrix<double> pMatrix, Matrix<double> qMatrix,
            Dictionary<User, List<Rating>> validationDataByUSer,Vector<double> bu,Vector<double> bi,double miu)
        {
            double rmse = 0;
            double squaredRatingSum = 0;
            double ratingCounter = 0;
            foreach (var user in validationDataByUSer)
            {
                foreach (var rating in user.Value)
                {
                    if (rating.RatedItem.MatrixIndex != -1)
                    {
                        squaredRatingSum += Math.Abs(rating.RatingNumber - PredictRating(pMatrix.Row(user.Key.MatrixIndex), qMatrix.Column(rating.RatedItem.MatrixIndex),bu[user.Key.MatrixIndex],bi[rating.RatedItem.MatrixIndex],miu));
                        ratingCounter++;
                    }
                }

            }

            rmse = squaredRatingSum / ratingCounter;
            return rmse;
        }


        
        private double RMSE(Matrix<double> pMatrix, Matrix<double> qMatrix,
            Dictionary<User, List<Rating>> validationDataByUSer,Vector<double> bu,Vector<double> bi,double miu)
        {
            double rmse = 0;
            double squaredRatingSum = 0;
            double ratingCounter = 0;
            foreach (var user in validationDataByUSer)
            {
                foreach (var rating in user.Value)
                {
                    if (rating.RatedItem.MatrixIndex != -1 && user.Key.MatrixIndex != -1)
                    {
                        squaredRatingSum += Math.Pow(rating.RatingNumber - PredictRating(pMatrix.Row(user.Key.MatrixIndex),qMatrix.Column(rating.RatedItem.MatrixIndex),bu[user.Key.MatrixIndex],bi[rating.RatedItem.MatrixIndex],miu), 2);
                        ratingCounter++;    
                    }
                    else
                        Console.WriteLine("Item: "+rating.RatedItem.ItemId+ " "+ rating.RatedItem.MatrixIndex +" user: "+ user.Key.UserId + " " + user.Key.MatrixIndex);
                }
                
            }

            rmse = Math.Sqrt(squaredRatingSum/ratingCounter);
            return rmse;
        }


        //split the trainmodel by percent parameter
            public
            void SplitDataToTrainAndValidation(double percentOfTrain,Dictionary<User,List<Rating>> allUsersToRating, Dictionary<Item,List<Rating>> allItemsToRating, out Dictionary<User,List<Rating>> trainDataByUser,
            out Dictionary<User,List<Rating>> validationDataByUSer,out Dictionary<int,User> allUsersByIndex, out Dictionary<int,Item> allItemsByIndex  )
            {
            var allCombinedData = allUsersToRating;
            var allCombinedItems = allItemsToRating;
            allUsersByIndex= new Dictionary<int, User>();
            allItemsByIndex = new Dictionary<int, Item>();
            trainDataByUser = new Dictionary<User, List<Rating>>();
            validationDataByUSer = new Dictionary<User, List<Rating>>();
            //Indexing Items and Users for matrices
            int i = 0;
            foreach (var user in allCombinedData)
            {
                allUsersByIndex.Add(user.Key.MatrixIndex, user.Key);
                //user.Key.MatrixIndex = i++;

            }

            int j = 0;
            foreach (var item in allCombinedItems)
            {
                allItemsByIndex.Add(item.Key.MatrixIndex,item.Key);
                //item.Key.MatrixIndex = j++;
            }
                
                var allRatings = allCombinedData.SelectMany(x => x.Value).ToList();
                int totalRatingsCount = allRatings.Count;
                Random rand = new Random();
                double c = 0;
                do
                {
                    int index = rand.Next(0, allRatings.Count - 1);
                    
                    List<Rating> ratingsOfUser = null;
                    if (trainDataByUser.TryGetValue(allRatings[index].RatingUser, out ratingsOfUser))
                    {
                        ratingsOfUser.Add(allRatings[index]);
                    }
                    else
                    {
                        ratingsOfUser = new List<Rating>();
                        ratingsOfUser.Add(allRatings[index]);
                        trainDataByUser.Add(allRatings[index].RatingUser,ratingsOfUser);
                    }
                    allRatings.RemoveAt(index);
                    c++;
                } while (c/totalRatingsCount < percentOfTrain);

                foreach (var rating in allRatings)
                {
                    List<Rating> ratingsOfUser = null;
                    if (validationDataByUSer.TryGetValue(rating.RatingUser, out ratingsOfUser))
                    {
                        ratingsOfUser.Add(rating);
                    }
                    else
                    {
                        ratingsOfUser = new List<Rating>();
                        ratingsOfUser.Add(rating);
                        validationDataByUSer.Add(rating.RatingUser,ratingsOfUser);
                    }
                }
            
        }


        public void TrainContentModel(int latentFeatures)
        {
            var allDataSeparatedByCategory = m_datamanager.GetAllTrainSeparatedData();
            var allItemsSeparatedByCategory = m_datamanager.GetAllTrainSeparatedItems();
            List<CategoryModel> allSubModels = new List<CategoryModel>();
            foreach (var category in allDataSeparatedByCategory)
            {
                if(allItemsSeparatedByCategory[category.Key].Count == 0)
                    continue;
                Dictionary<User, List<Rating>> trainDataByUser;
                Dictionary<User, List<Rating>> validationDataByUser;
                Dictionary<int, User> allUsersByIndex;
                Dictionary<int, Item> allItemsByIndex;
                if(!allItemsSeparatedByCategory[category.Key].SelectMany(x => x.Value).Any())
                    continue;
                SplitDataToTrainAndValidation(0.8,category.Value,allItemsSeparatedByCategory[category.Key],out trainDataByUser,out validationDataByUser, out allUsersByIndex,out allItemsByIndex);
                CategoryModel cm = new CategoryModel(m_datamanager,category.Key,latentFeatures,0.001,0.1,trainDataByUser,validationDataByUser,allUsersByIndex,allItemsByIndex);
                allSubModels.Add(cm);

                var traincount = trainDataByUser.Values.Sum(x => x.Count);
                var validationcount = validationDataByUser.Values.Sum(x => x.Count);
                Console.WriteLine("train count = " + traincount + ", validation count = " + validationcount);
                Console.WriteLine(((double)traincount) / (traincount + validationcount));

                double prevRMSE = RMSE(cm.PMatrix, cm.QMatrix, cm.ValidationDataByUser, cm.Bu, cm.Bi, cm.Miu);
                double currentRMSE = prevRMSE;
                int ccc = 0;
                int dd = 0;


                for (int i = 0; i < cm.LatentFeatures; i++)
                {
                    ccc = 0;
                    for (int epoch = 0; epoch < 100; epoch++)
                    {
                        prevRMSE = currentRMSE;
                        foreach (var user in cm.TrainDataByUser)
                        {
                            foreach (var rating in user.Value)
                            {
                                if (rating.RatedItem.MatrixIndex != -1)
                                {
                                    double err_ui = rating.RatingNumber -
                                                    PredictRating(cm.PMatrix.Row(user.Key.MatrixIndex),
                                                        cm.QMatrix.Column(rating.RatedItem.MatrixIndex), cm.Bu[user.Key.MatrixIndex], cm.Bi[rating.RatedItem.MatrixIndex], cm.Miu);

                                    cm.Bu[user.Key.MatrixIndex] += cm.LearningRate* (err_ui - cm.Regularization* cm.Bu[user.Key.MatrixIndex]);
                                    cm.Bi[rating.RatedItem.MatrixIndex] += cm.LearningRate *
                                                                        (err_ui *
                                                                         cm.QMatrix[i, rating.RatedItem.MatrixIndex
                                                                             ] -
                                                                         cm.Regularization *
                                                                         cm.PMatrix[user.Key.MatrixIndex, i]);
                                    cm.QMatrix[i, rating.RatedItem.MatrixIndex] += cm.LearningRate *
                                                                                (err_ui *
                                                                                 cm.PMatrix[user.Key.MatrixIndex, i
                                                                                     ] -
                                                                                 cm.Regularization *
                                                                                 cm.QMatrix[
                                                                                     i,
                                                                                     rating.RatedItem.MatrixIndex]);
                                }
                            }
                        }
                        currentRMSE = RMSE(cm.PMatrix, cm.QMatrix, validationDataByUser, cm.Bu, cm.Bi, cm.Miu);
                        if (currentRMSE > prevRMSE)
                        {
                            //Console.WriteLine("Feature number: " + i + "    Number of iterations: " + ccc);
                            break;
                        }

                        ccc++;
                    }
                    //Console.WriteLine("Feature number: " + i + "    Number of iterations: " + ccc);
              }
                cm.MinTrainRMSE = prevRMSE;
                cm.MinTestRMSE = RMSE(cm.PMatrix, cm.QMatrix,
                    m_datamanager.GetAllTestSetSeparatedData()[category.Key], cm.Bu, cm.Bi, cm.Miu);
                cm.MinTestMAE = MAE(cm.PMatrix, cm.QMatrix, m_datamanager.GetAllTestSetSeparatedData()[category.Key],
                    cm.Bu, cm.Bi, cm.Miu);

                Console.WriteLine("Category : "+category.Key+" Train RMSE: "+cm.MinTrainRMSE +" Test RMSE: "+cm.MinTestRMSE +" Test MAE: "+ cm.MinTestMAE);
                }
            Console.WriteLine("Avarage Train RMSE: "+ allSubModels.Average(x=>x.MinTrainRMSE));
            Console.WriteLine("Avarage Test RMSE: " + allSubModels.Average(x => x.MinTestRMSE));
            Console.WriteLine("Avarage Test MAE: " + allSubModels.Average(x => x.MinTestMAE));

            //before your loop
            var csv = new StringBuilder();

            //Suggestion made by KyleMit
            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",
               allSubModels[0].MinTrainRMSE, allSubModels[0].MinTestRMSE, allSubModels[0].MinTestMAE,
               allSubModels[1].MinTrainRMSE, allSubModels[1].MinTestRMSE, allSubModels[1].MinTestMAE,
               allSubModels[2].MinTrainRMSE, allSubModels[2].MinTestRMSE, allSubModels[2].MinTestMAE,
            allSubModels[3].MinTrainRMSE, allSubModels[3].MinTestRMSE, allSubModels[3].MinTestMAE,
            allSubModels[4].MinTrainRMSE, allSubModels[4].MinTestRMSE, allSubModels[4].MinTestMAE,
            allSubModels[5].MinTrainRMSE, allSubModels[5].MinTestRMSE, allSubModels[5].MinTestMAE,
            allSubModels[6].MinTrainRMSE, allSubModels[6].MinTestRMSE, allSubModels[6].MinTestMAE
            );
            csv.AppendLine(newLine);

            //after your loop
            File.WriteAllText(@"C:\Users\Shimon\Desktop\content.csv", csv.ToString());
        }
    }
}
