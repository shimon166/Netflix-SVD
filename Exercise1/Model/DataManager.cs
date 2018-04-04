using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exercise1.Model
{
    public class DataManager
    {
        private DataReader m_dataReader;
        public DataManager()
        {
            m_dataReader = DataReader.GetInstance();
        }


        public void Load(bool dTrainAllCategories = false)
        {
            m_dataReader.Load(dTrainAllCategories);
        }

        #region combined categories getters

        public Dictionary<User, List<Rating>> GetAllTrainCombinedData()
        {
            return m_dataReader.GetAllCombinedData();
        }

        public Dictionary<Item, List<Rating>> GetAllTrainCombinedItems()
        {
            return m_dataReader.GetAllCombinedItems();
        }


        public Dictionary<User, List<Rating>> GetAllTestSetCombinedData()
        {
            return m_dataReader.GetAllTestSetCombinedData();
        }

        public Dictionary<Item, List<Rating>> GetAllTestSetCombinedItems()
        {
            return m_dataReader.GetAllTestSetCombinedItems();
        }

        #endregion

        #region separated categories getters

        public Dictionary<string,Dictionary<User, List<Rating>>> GetAllTrainSeparatedData()
        {
            return m_dataReader.GetAllTrainSeparatedData();
        }

        public Dictionary<string, Dictionary<Item, List<Rating>>> GetAllTrainSeparatedItems()
        {
            return m_dataReader.GetAllTrainSeparatedItems();
        }


        public Dictionary<string,Dictionary<User, List<Rating>>> GetAllTestSetSeparatedData()
        {
            return m_dataReader.GetAllTestSetSeparatedData();
        }

        public Dictionary<string,Dictionary<Item, List<Rating>>> GetAllTestSetSeparatedItems()
        {
            return m_dataReader.GetAllTestSetSeparatedItems();
        }


        #endregion

    }
}
