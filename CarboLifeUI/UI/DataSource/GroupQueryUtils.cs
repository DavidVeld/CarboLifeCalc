using CarboLifeAPI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CarboLifeUI.UI
{
    internal class GroupQueryUtils
    {
        public List<CarboElement> ElementList { get; }
        public string QueryGroup { get; }
        public string Query { get; }
        public bool Result { get; internal set; }
        public ObservableCollection<CarboElement> PassedElementList { get; internal set; }
        public ObservableCollection<CarboElement> FilteredElementList { get; internal set; }

        private DataTable fullTable;
        private DataTable passedTable;
        private DataTable filteredTable;


        public GroupQueryUtils(List<CarboElement> elementList, string queryGroup, string query)
        {
            ElementList = elementList;
            QueryGroup = queryGroup;
            Query = query;
            Result = false;

        }

        /// <summary>
        /// The Main entry poiint that calls the search, if sucessfull "result" will be set as true and passedElementList and filteredElementList fill show data
        /// </summary>
        internal void TrySearch()
        {
            Result = false;

            //First convert the main element list to a DataTable, then convert the query to SQL query. Get result table and then convert them back to observable

            try
            {
                fullTable = getDataTablefromList(ElementList);

                genericSearch();

                //Search was done, place the items that were NOT found in another list;

            }
            catch(Exception ex)
            {

            }

            Result = true;

        }


        private void genericSearch()
        {

            PassedElementList = new ObservableCollection<CarboElement>();
            FilteredElementList = new ObservableCollection<CarboElement>();

            foreach (CarboElement el in ElementList)
            {
                bool itemFound = false;

                foreach (var prop in el.GetType().GetProperties())
                {
                    try
                    {
                        if (prop.Name == QueryGroup)
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                //string search
                                string valueString = prop.GetValue(el, null) as string;
                                valueString = valueString.ToLower();
                                if (valueString.Contains(Query.ToLower()))
                                {
                                    FilteredElementList.Add(el);
                                    itemFound = true;
                                    break;
                                }
                            }
                            else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(int))
                            {
                                //number search
                                var valueBool = prop.GetValue(el, null);
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                //bool search
                                var valueBool = prop.GetValue(el, null);
                            }
                            else
                            {
                                //no search
                            }
                        }
                    }
                    catch (Exception ex)
                {

                }
            }

                //If the item was not found, place it int the PassedElementList
                if (itemFound == false)
                    PassedElementList.Add(el);
            }
        }

        private DataTable getDataTablefromList<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }


    }
}