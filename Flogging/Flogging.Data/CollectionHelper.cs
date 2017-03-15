using System;
using System.Collections.Generic;
using System.Data;

namespace Flogging.Data
{
    public class CollectionHelper
    {
        public static List<T> BuildCollection<T>(Type type, DataTable table)
        {
            var ret = new List<T>();

            //Get all the properties in Entity Class
            var props = type.GetProperties();

            foreach (DataRow row in table.Rows)
            {
                //Create new instance of Entity
                var entity = Activator.CreateInstance<T>();

                //Set all properties from the column names
                //NOTE: This assumes your column names are the same name as your class property names
                foreach (var col in props)
                {
                    if (!table.Columns.Contains(col.Name))
                        continue;
                    try
                    {
                        if (row[col.Name].Equals(DBNull.Value))
                        {
                            col.SetValue(entity, null);
                        }
                        else
                        {
                            if (col.PropertyType == typeof(bool) || col.PropertyType == typeof(bool?))
                            {
                                // straight-up bool types
                                col.SetValue(entity, (row[col.Name].ToString() == "1" || row[col.Name].ToString() == bool.TrueString));
                            }
                            else if (col.PropertyType == typeof(int) || col.PropertyType == typeof(short) ||
                                        col.PropertyType == typeof(long) || col.PropertyType == typeof(double) || col.PropertyType == typeof(decimal))
                            {
                                // numeric types (non-nullable)
                                col.SetValue(entity, string.IsNullOrEmpty(row[col.Name].ToString())
                                                        ? 0
                                                        : Convert.ChangeType(row[col.Name].ToString(), col.PropertyType)
                                            , null);
                            }
                            else
                            {
                                if (col.PropertyType.Name.StartsWith("Nullable"))
                                {
                                    var colType = Nullable.GetUnderlyingType(col.PropertyType);
                                    if (colType == typeof(int) || colType == typeof(short) ||
                                        colType == typeof(long) ||
                                        colType == typeof(double) || colType == typeof(decimal) ||
                                        colType.IsEnum)
                                    {
                                        if (row[col.Name].Equals(DBNull.Value))
                                            // numeric nullables
                                            col.SetValue(entity, null, null);
                                        else if (colType.IsEnum)
                                            col.SetValue(entity, Enum.Parse(colType, row[col.Name].ToString()));
                                        else
                                            col.SetValue(entity, Convert.ChangeType(row[col.Name].ToString(), colType), null);
                                    }
                                    else if (colType == typeof(bool))
                                        col.SetValue(entity, row[col.Name].ToString() == "1");
                                    else
                                    {
                                        //non-numeric nullables
                                        col.SetValue(entity, Convert.ChangeType(row[col.Name].ToString(), Nullable.GetUnderlyingType(col.PropertyType)), null);
                                    }
                                }
                                else
                                {
                                    col.SetValue(entity, col.PropertyType.IsEnum
                                                            ? Enum.Parse(col.PropertyType, row[col.Name].ToString()) // enum logic
                                                            : Convert.ChangeType(row[col.Name].ToString(), col.PropertyType), null); // non-nullable, non-numeric, non-enums
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("Failed building collection. Setting Property{0} to value {1}", col.Name, row[col.Name]), ex);
                    }
                }

                ret.Add(entity);
            }

            return ret;
        }
    }
}
