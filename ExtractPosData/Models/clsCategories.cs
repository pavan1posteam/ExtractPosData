using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExtractPosData.Model
{
    public class clsCategories
    {
        [XmlRoot(ElementName = "category")]
        public class Category
        {
            [XmlElement(ElementName = "id")]
            public string Id { get; set; }
            [XmlElement(ElementName = "name")]
            public string Name { get; set; }
            [XmlElement(ElementName = "descr")]
            public string Descr { get; set; }
            [XmlElement(ElementName = "pathString")]
            public string PathString { get; set; }
        }

        [XmlRoot(ElementName = "item")]
        public class Item
        {
            [XmlElement(ElementName = "category")]
            public Category Category { get; set; }
            [XmlElement(ElementName = "child_categories")]
            public Child_categories Child_categories { get; set; }
        }

        [XmlRoot(ElementName = "child_categories")]
        public class Child_categories
        {
            [XmlElement(ElementName = "item")]
            public List<Item> Item { get; set; }
        }

        [XmlRoot(ElementName = "result")]

        public class Result
        {
            [XmlElement(ElementName = "category")]
            public Category Category { get; set; }
            [XmlElement(ElementName = "child_categories")]
            public Child_categories Child_categories { get; set; }
        }
    }
}
