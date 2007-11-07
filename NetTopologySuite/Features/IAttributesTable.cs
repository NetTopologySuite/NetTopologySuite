using System;

namespace GisSharpBlog.NetTopologySuite.Features
{
    public interface IAttributesTable
    {
        void AddAttribute(string attributeName, object value);

        void DeleteAttribute(string attributeName);

        Type GetType(string attributeName);

        object this[string attributeName] { get; set; }

        Boolean Exists(string attributeName);

        Int32 Count { get; }

        string[] GetNames();

        object[] GetValues();
    }
}