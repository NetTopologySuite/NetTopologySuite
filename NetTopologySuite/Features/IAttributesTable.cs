using System;

namespace GisSharpBlog.NetTopologySuite.Features
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAttributesTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        void AddAttribute(string attributeName, object value);               
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeName"></param>
        void DeleteAttribute(string attributeName);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        Type GetType(string attributeName);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        object this[string attributeName] { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        bool Exists(string attributeName);

        /// <summary>
        /// 
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string[] GetNames();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object[] GetValues();
    }
}
