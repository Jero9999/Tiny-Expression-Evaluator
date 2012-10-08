using System;
using System.Collections.Generic;
using System.Data;

namespace Formy.Evaluation.Test
{
    public static class TestUtils
    {
        public static object GetTestContext()
        {
            var dt = new DataTable("table01A");
            dt.Columns.AddRange(new[]
                                    {
                                        new DataColumn("col1"), 
                                        new DataColumn("col2"), 
                                        new DataColumn("col3"),
                                        new DataColumn("col4"),
                                        new DataColumn("col5")
                                    });
            dt.Rows.Add("one", 1, DateTime.Today.AddYears(1), true, 12.5m);
            dt.Rows.Add("two", 2, DateTime.Today.AddYears(2), false, 22.5m);
            dt.Rows.Add("three", 3, DateTime.Today.AddYears(3), true, 49.9999m);
            dt.Rows.Add("four", 4, new DateTime(2012, 12, 12, 12, 12, 12), false, 0.125m);

            return new Dictionary<string, object>
                       {
                           {"x",42},
                           {"table01A", dt},
                           {"array01A", new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}},
                           {"elizabeth", new Person{ Name = "Elizabeth", Age=27, Married = true }},
                           {"lookup", new Dictionary<string, object> {{"key1", 1}, {"key2", null}}},
                           {"echo", new Person{Name = "echo"}},
                           {"$usr", new Person{Name = "Anonymous", Age=31337}},
                       };
        }
    }
}