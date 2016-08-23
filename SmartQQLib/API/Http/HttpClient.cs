﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SmartQQLib.API.Http
{



    public class HttpClient
    {

        /// <summary>
        /// 访问服务器时的cookies
        /// </summary>
        private CookieContainer mCookiesContainer;

        /// <summary>
        /// 默认UserAgent
        /// </summary>
        private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
        private static readonly string DefaultContentType = "application/x-www-form-urlencoded";
        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreateGetHttpResponse(string url, string referer, int? timeout, string userAgent)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (mCookiesContainer == null)
            {
                mCookiesContainer = new CookieContainer();
            }
            request.CookieContainer = mCookiesContainer;  //启用cookie
            if (referer != null)
            {
                request.Referer = referer;
            }

            return request.GetResponse() as HttpWebResponse;
        }
        public string GET(string url, string referer)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)CreateGetHttpResponse(url, referer, null, null);

                Stream respStream = response.GetResponseStream();
                // Dim reader As StreamReader = New StreamReader(respStream)
                using (StreamReader reader = new StreamReader(respStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return "";
        }

        public Image GetImage(string url, string referer)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)CreateGetHttpResponse(url, referer, null, null);

                Stream respStream = response.GetResponseStream();
                return Image.FromStream(respStream);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return null;
        }



        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreatePostHttpResponse(string url, string referer, string contenttype,IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                throw new ArgumentNullException("requestEncoding");
            }
            HttpWebRequest request = null;

            request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = "POST";

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }
            if (!string.IsNullOrEmpty(contenttype))
            {
                request.ContentType = contenttype;
            }
            else
            {
                request.ContentType = DefaultContentType;
            }

            
            if (referer != null)
            {
                request.Referer = referer;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (mCookiesContainer == null)
            {
                mCookiesContainer = new CookieContainer();
            }
            request.CookieContainer = mCookiesContainer;  //启用cookie
            //如果需要POST数据  

            if (!(parameters == null || parameters.Count == 0))
            {

                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        if (contenttype == DefaultContentType)
                        {

                            buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}", parameters[key]);
                        }

                    }
                    i++;


                }
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }



        public string POST(string url, string referer, string contenttype, IDictionary<string, string> parameters)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)CreatePostHttpResponse(url, referer, contenttype, parameters, null, null, Encoding.UTF8);

                Stream respStream = response.GetResponseStream();
                // Dim reader As StreamReader = New StreamReader(respStream)
                using (StreamReader reader = new StreamReader(respStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return "";
        }
        /// <summary>
        /// 列出cookie
        /// </summary>
        public string ListCookie()
        {
            List<Cookie> cookies = GetAllCookies(mCookiesContainer);
            foreach (Cookie c in cookies)
            {
                System.Diagnostics.Debug.WriteLine(c);
            }
            return null;
        }



        /// <summary>
        /// 获取指定cookie
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetCookie(string name)
        {
            List<Cookie> cookies = GetAllCookies(mCookiesContainer);
            foreach (Cookie c in cookies)
            {
                if (c.Name == name)
                {
                    return (c.ToString()).Remove(0, name.Length + 1);
                }
            }
            return null;
        }

        private static List<Cookie> GetAllCookies(CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) lstCookies.Add(c);
            }
            return lstCookies;
        }


    }
}
