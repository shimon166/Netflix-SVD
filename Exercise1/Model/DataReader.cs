using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;

namespace Exercise1.Model
{
    public class DataReader
    {
        private static DataReader m_instance = null;

        private const string m_dataRootPath = "\\Data\\";

        #region Separated Data Structures

        private bool m_bSeparatedDataLoaded = false;
        private Dictionary<string, Dictionary<User, List<Rating>>>  m_TrainsetSeparatedCategoriesCategoryToUsersRating;
        private Dictionary<string, Dictionary<Item, List<Rating>>> m_TrainsetItemsToRatingsMap;

        private Dictionary<string, Dictionary<User, List<Rating>>> m_TestsetSeparatedCategoriesCategoryToUsersRating;
        private Dictionary<string, Dictionary<Item, List<Rating>>> m_TestsetItemsToRatingsMap;
        #endregion

        #region Merged Data Structures
        private bool m_bMergedDataLoaded = false;
        private Dictionary<User, List<Rating>> m_MergedCategoriesTrainsetUserToRatings;
        private Dictionary<Item, List<Rating>> m_MergedCategoriesTrainsetItemsToRatingsMap;


        private Dictionary<User, List<Rating>> m_MergedCategoriesTestsetUserToRatings;
        private Dictionary<Item, List<Rating>> m_MergedCategoriesTestsetItemsToRatingsMap;
        #endregion

        private DataReader()
        {
            m_TrainsetSeparatedCategoriesCategoryToUsersRating = new Dictionary<string, Dictionary<User, List<Rating>>>();
            m_TrainsetItemsToRatingsMap = new Dictionary<string, Dictionary<Item, List<Rating>>>();

            m_TestsetSeparatedCategoriesCategoryToUsersRating = new Dictionary<string, Dictionary<User, List<Rating>>>();
            m_TestsetItemsToRatingsMap = new Dictionary<string, Dictionary<Item, List<Rating>>>();

            m_MergedCategoriesTrainsetUserToRatings = new Dictionary<User, List<Rating>>();
            m_MergedCategoriesTrainsetItemsToRatingsMap = new Dictionary<Item, List<Rating>>();

            m_MergedCategoriesTestsetUserToRatings = new Dictionary<User, List<Rating>>();
            m_MergedCategoriesTestsetItemsToRatingsMap = new Dictionary<Item, List<Rating>>();
        
        }


        public static DataReader GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new DataReader();
            }
            return m_instance;
        }

        public List<string> GetAllCategoriesFromDisk()
        {
            string[] folders = Directory.GetDirectories(Environment.CurrentDirectory+m_dataRootPath);
            return folders.ToList();
        }

        //train an test files format: UserID::ItemID::Rating
        public void Load(bool dTrainAllCategories = false)
        {
            if (dTrainAllCategories)
            {
                LoadAllMergedCategories();
            }
            else
            {
                if (m_bSeparatedDataLoaded)
                    return;
                string dataPath = Environment.CurrentDirectory + m_dataRootPath;
                int globalIndex = 0;

                Dictionary<string, Item> tmpDictForItem = new Dictionary<string, Item>();
                Dictionary<string, User> tmpDictForUser = new Dictionary<string, User>();
                int userIndex = 0;
                int itemIndex = 0;
                //iterate over each category in data
                foreach (var cat in Directory.GetDirectories(dataPath))
                {
                    userIndex = 0;
                    itemIndex = 0;
                    string category = cat.Substring(cat.LastIndexOf("\\") + 1);
                    tmpDictForItem.Clear();
                    tmpDictForUser.Clear();
                    using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" +"users.txt"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            string[] separator = new[] {":"};
                            string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            User u = new User(Convert.ToInt32(separated[0]), category);
                            u.MatrixIndex = userIndex++;
                            tmpDictForUser.Add(Convert.ToInt32(separated[0]) + "" + category, u);
                        }
                    }
                    using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + "items.txt"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            string[] separator = new[] { "::" };
                            string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            Item i = new Item(Convert.ToInt32(separated[0]), category);
                            i.MatrixIndex = itemIndex++;
                            tmpDictForItem.Add(Convert.ToInt32(separated[0]) + "" + category, i);
                        }
                    }

                    #region Load all training data from files
                    Dictionary<User, List<Rating>> allUsers = new Dictionary<User, List<Rating>>();
                    foreach (var user in tmpDictForUser.Values)
                    {
                        allUsers.Add(user,new List<Rating>());
                    }
                    m_TrainsetSeparatedCategoriesCategoryToUsersRating.Add(category,allUsers);
                    
                    Dictionary<Item, List<Rating>> allItems = new Dictionary<Item, List<Rating>>();
                    foreach (var item in tmpDictForItem.Values)
                    {
                        allItems.Add(item, new List<Rating>());
                    }
                    m_TrainsetItemsToRatingsMap.Add(category,allItems);

                    //open xxx_training.txt file
                    // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + category + "_training.txt"))
                    {
                        // Read the stream to a string, and write the string to the console.
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            string[] separator = new[] {"::"};
                            string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            if (Convert.ToInt32(separated[0]) != -1 && Convert.ToInt32(separated[1]) != -1)
                            {
                                User u = null;
                                if (!tmpDictForUser.TryGetValue(Convert.ToInt32(separated[0]) + "" + category, out u))
                                {
                                    u = new User(Convert.ToInt32(separated[0]), category);
                                    tmpDictForUser.Add(Convert.ToInt32(separated[0]) + "" + category, u);
                                }

                                if (!allUsers.ContainsKey(u))
                                {
                                    
                                    List<Rating> ratings = new List<Rating>();
                                    Rating r = new Rating();
                                    r.RatingNumber = Convert.ToInt32(separated[2]);
                                    r.RatingUser = u;
                                    Item i = null;
                                    if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                    {
                                        i = new Item(Convert.ToInt32(separated[1]), category);
                                        tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                    }


                                    r.RatedItem = i;
                                    ratings.Add(r);
                                    allUsers.Add(u, ratings);


                                    if (allItems.TryGetValue(i, out ratings))
                                    {
                                        ratings.Add(r);
                                    }
                                    else
                                    {
                                        ratings = new List<Rating>();
                                        ratings.Add(r);
                                        allItems.Add(i, ratings);
                                    }
                                }
                                else
                                {
                                    List<Rating> ratings;
                                    allUsers.TryGetValue(u, out ratings);
                                    Rating r = new Rating();
                                    r.RatingNumber = Convert.ToInt32(separated[2]);
                                    Item i = null;
                                    if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                    {
                                        i = new Item(Convert.ToInt32(separated[1]), category);
                                        tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                    }
                                    r.RatingUser = u;
                                    r.RatedItem = i;
                                    ratings.Add(r);

                                    if (allItems.TryGetValue(i, out ratings))
                                    {
                                        ratings.Add(r);
                                    }
                                    else
                                    {
                                        ratings = new List<Rating>();
                                        ratings.Add(r);
                                        allItems.Add(i, ratings);
                                    }

                                }
                            }
                            
                        }
                    }
                    #endregion

                    #region Load all test data from file
                    allUsers = new Dictionary<User, List<Rating>>();
                    foreach (var user in tmpDictForUser.Values)
                    {
                        allUsers.Add(user, new List<Rating>());
                    }
                    m_TestsetSeparatedCategoriesCategoryToUsersRating.Add(category, allUsers);

                    allItems = new Dictionary<Item, List<Rating>>();
                    foreach (var item in tmpDictForItem.Values)
                    {
                        allItems.Add(item, new List<Rating>());
                    }
                    m_TestsetItemsToRatingsMap.Add(category,allItems);
                    //open xxx_test.txt file
                    // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + category + "_test.txt"))
                    {
                        // Read the stream to a string, and write the string to the console.
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            string[] separator = new[] {"::"};
                            string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            if (Convert.ToInt32(separated[0]) != -1 && Convert.ToInt32(separated[1]) != -1)
                            {
                                User u = null;
                                if (!tmpDictForUser.TryGetValue(Convert.ToInt32(separated[0]) + "" + category, out u))
                                {
                                    u = new User(Convert.ToInt32(separated[0]), category);
                                    tmpDictForUser.Add(Convert.ToInt32(separated[0]) + "" + category, u);
                                }

                                if (!allUsers.ContainsKey(u))
                                {
                                    
                                    List<Rating> ratings = new List<Rating>();
                                    Rating r = new Rating();
                                    r.RatingNumber = Convert.ToInt32(separated[2]);
                                    r.RatingUser = u;
                                    Item i = null;
                                    if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                    {
                                        i = new Item(Convert.ToInt32(separated[1]), category);
                                        tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                    }


                                    r.RatedItem = i;
                                    ratings.Add(r);
                                    allUsers.Add(u, ratings);


                                    if (allItems.TryGetValue(i, out ratings))
                                    {
                                        ratings.Add(r);
                                    }
                                    else
                                    {
                                        ratings = new List<Rating>();
                                        ratings.Add(r);
                                        allItems.Add(i, ratings);
                                    }
                                }
                                else
                                {
                                    List<Rating> ratings;
                                    allUsers.TryGetValue(u, out ratings);
                                    Rating r = new Rating();
                                    r.RatingNumber = Convert.ToInt32(separated[2]);
                                    Item i = null;
                                    if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                    {
                                        i = new Item(Convert.ToInt32(separated[1]), category);
                                        tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                    }
                                    r.RatingUser = u;
                                    r.RatedItem = i;
                                    ratings.Add(r);

                                    if (allItems.TryGetValue(i, out ratings))
                                    {
                                        ratings.Add(r);
                                    }
                                    else
                                    {
                                        ratings = new List<Rating>();
                                        ratings.Add(r);
                                        allItems.Add(i, ratings);
                                    }

                                }
                            }
                       }

                    }
                    #endregion
                }
                m_bSeparatedDataLoaded = true;
            }
            
        }

        private void LoadAllMergedCategories()
        {
            if (m_bMergedDataLoaded)
                    return;
            Dictionary<string, Item> tmpDictForItem = new Dictionary<string, Item>();
            Dictionary<string,User> tmpDictForUser = new Dictionary<string, User>();
            string dataPath = Environment.CurrentDirectory + m_dataRootPath;
            int userIndex = 0;
            int itemIndex = 0;
            //iterate over each category in data
            foreach (var cat in Directory.GetDirectories(dataPath))
            {
                string category = cat.Substring(cat.LastIndexOf("\\") + 1);
                using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + "users.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] separator = new[] { ":" };
                        string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        User u = new User(Convert.ToInt32(separated[0]), category);
                        u.MatrixIndex = userIndex++;
                        tmpDictForUser.Add(Convert.ToInt32(separated[0]) + "" + category, u);
                        m_MergedCategoriesTrainsetUserToRatings.Add(u,new List<Rating>());
                        m_MergedCategoriesTestsetUserToRatings.Add(u,new List<Rating>());
                    }
                }
                using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + "items.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] separator = new[] { "::" };
                        string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        Item i = new Item(Convert.ToInt32(separated[0]), category);
                        i.MatrixIndex = itemIndex++;
                        tmpDictForItem.Add(Convert.ToInt32(separated[0]) + "" + category, i);
                        m_MergedCategoriesTrainsetItemsToRatingsMap.Add(i,new List<Rating>());
                        m_MergedCategoriesTestsetItemsToRatingsMap.Add(i,new List<Rating>());
                    }
                }
                #region Load Merged Trainset
                //open xxx_training.txt file
                // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + category + "_training.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] separator = new[] { "::" };
                        string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if (Convert.ToInt32(separated[0]) != -1 && Convert.ToInt32(separated[1]) != -1)
                        {
                            User u = null;
                            if (!tmpDictForUser.TryGetValue(Convert.ToInt32(separated[0]) + "" + category, out u))
                            {
                                u = new User(Convert.ToInt32(separated[0]), category);
                                tmpDictForUser.Add(Convert.ToInt32(separated[0]) + "" + category, u);
                            }

                            if (!m_MergedCategoriesTrainsetUserToRatings.ContainsKey(u))
                            {
                                
                                List<Rating> ratings = new List<Rating>();
                                Rating r = new Rating();
                                r.RatingNumber = Convert.ToInt32(separated[2]);
                                r.RatingUser = u;
                                Item i = null;
                                if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                {
                                    i = new Item(Convert.ToInt32(separated[1]), category);
                                    tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                }
                                    
                                
                                r.RatedItem = i;
                                ratings.Add(r);
                                m_MergedCategoriesTrainsetUserToRatings.Add(u, ratings);


                                if (m_MergedCategoriesTrainsetItemsToRatingsMap.TryGetValue(i, out ratings))
                                {
                                    ratings.Add(r);
                                }
                                else
                                {
                                    ratings = new List<Rating>();
                                    ratings.Add(r);
                                    m_MergedCategoriesTrainsetItemsToRatingsMap.Add(i, ratings);
                                }
                            }
                            else
                            {
                                List<Rating> ratings;
                                m_MergedCategoriesTrainsetUserToRatings.TryGetValue(u, out ratings);
                                Rating r = new Rating();
                                r.RatingNumber = Convert.ToInt32(separated[2]);
                                Item i = null;
                                if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                {
                                    i = new Item(Convert.ToInt32(separated[1]), category);
                                    tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                }
                                r.RatingUser = u;
                                r.RatedItem = i;
                                ratings.Add(r);

                                if (m_MergedCategoriesTrainsetItemsToRatingsMap.TryGetValue(i, out ratings))
                                {
                                    ratings.Add(r);
                                }
                                else
                                {
                                    ratings = new List<Rating>();
                                    ratings.Add(r);
                                    m_MergedCategoriesTrainsetItemsToRatingsMap.Add(i, ratings);
                                }

                            }
                        }
                    }
                }
#endregion

                #region Load Merged Testset

                //open xxx_test.txt file
                // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(dataPath + "\\" + category + "\\" + category + "_test.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] separator = new[] { "::" };
                        string[] separated = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if (Convert.ToInt32(separated[0]) != -1 && Convert.ToInt32(separated[1]) != -1)
                        {
                            User u = null;
                            if (!tmpDictForUser.TryGetValue(Convert.ToInt32(separated[0]) + "" + category, out u))
                            {
                                u = new User(Convert.ToInt32(separated[0]), category);
                                tmpDictForUser.Add(Convert.ToInt32(separated[0]) + "" + category, u);
                            }

                            if (!m_MergedCategoriesTestsetUserToRatings.ContainsKey(u))
                            {
                                
                                List<Rating> ratings = new List<Rating>();
                                Rating r = new Rating();
                                r.RatingNumber = Convert.ToInt32(separated[2]);
                                r.RatingUser = u;
                                Item i = null;
                                if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                {
                                    i = new Item(Convert.ToInt32(separated[1]), category);
                                    tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                }


                                r.RatedItem = i;
                                ratings.Add(r);
                                m_MergedCategoriesTestsetUserToRatings.Add(u, ratings);


                                if (m_MergedCategoriesTestsetItemsToRatingsMap.TryGetValue(i, out ratings))
                                {
                                    ratings.Add(r);
                                }
                                else
                                {
                                    ratings = new List<Rating>();
                                    ratings.Add(r);
                                    m_MergedCategoriesTestsetItemsToRatingsMap.Add(i, ratings);
                                }
                            }
                            else
                            {
                                List<Rating> ratings;
                                m_MergedCategoriesTestsetUserToRatings.TryGetValue(u, out ratings);
                                Rating r = new Rating();
                                r.RatingNumber = Convert.ToInt32(separated[2]);
                                Item i = null;
                                if (!tmpDictForItem.TryGetValue(Convert.ToInt32(separated[1]) + "" + category, out i))
                                {
                                    i = new Item(Convert.ToInt32(separated[1]), category);
                                    tmpDictForItem.Add(Convert.ToInt32(separated[1]) + "" + category, i);
                                }
                                r.RatingUser = u;
                                r.RatedItem = i;
                                ratings.Add(r);

                                if (m_MergedCategoriesTestsetItemsToRatingsMap.TryGetValue(i, out ratings))
                                {
                                    ratings.Add(r);
                                }
                                else
                                {
                                    ratings = new List<Rating>();
                                    ratings.Add(r);
                                    m_MergedCategoriesTestsetItemsToRatingsMap.Add(i, ratings);
                                }

                            }
                        }
                    }

                }

                #endregion


            }


            m_bMergedDataLoaded = true;
        }

        public Dictionary<User, List<Rating>> GetAllCombinedData()
        {
            return m_MergedCategoriesTrainsetUserToRatings;
        }

        public Dictionary<Item, List<Rating>> GetAllCombinedItems()
        {
            return m_MergedCategoriesTrainsetItemsToRatingsMap;
        }

        public Dictionary<User, List<Rating>> GetAllTestSetCombinedData()
        {
            return m_MergedCategoriesTestsetUserToRatings;
        }

        public Dictionary<Item, List<Rating>> GetAllTestSetCombinedItems()
        {
            return m_MergedCategoriesTestsetItemsToRatingsMap;
        }



        public Dictionary<string,Dictionary<User, List<Rating>>> GetAllTrainSeparatedData()
        {
            return m_TrainsetSeparatedCategoriesCategoryToUsersRating;
        }

        public Dictionary<string,Dictionary<Item, List<Rating>>> GetAllTrainSeparatedItems()
        {
            return m_TrainsetItemsToRatingsMap;
        }

        public Dictionary<string,Dictionary<User, List<Rating>>> GetAllTestSetSeparatedData()
        {
            return m_TestsetSeparatedCategoriesCategoryToUsersRating;
        }

        public Dictionary<string,Dictionary<Item, List<Rating>>> GetAllTestSetSeparatedItems()
        {
            return m_TestsetItemsToRatingsMap;
        }

    }
}
