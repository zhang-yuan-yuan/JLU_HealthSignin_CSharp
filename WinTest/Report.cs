using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLUReportOperator
{
    class Report
    {
        public static int jlusign_in(string jsonFilePath)
        {
            //string jsonFilePath = "../../student-info.json";
            string jsonString = File.ReadAllText(jsonFilePath);
            var json = JsonConvert.DeserializeObject<Dictionary<object, object>>(jsonString);
            var usersArray = json["users"] as JArray;
            foreach (var userString in usersArray)
            {
                var user = JsonConvert.DeserializeObject<Dictionary<object, object>>(userString.ToString());
                try
                {
                    HttpClient httpClient = InitHttpClient(new KeyValuePair<string, string>("Referer", "https://ehall.jlu.edu.cn/"));
                    StudentSignIn(ref user, ref httpClient);
                    string csrfToken = GetCSRFToken(ref httpClient, json["transaction"].ToString());
                    string sid = Start(ref httpClient, json["transaction"].ToString(), csrfToken);

                    var entity = GetEntityByRender(ref httpClient, sid, csrfToken);
                    var formDataDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(entity["data"].ToString());
                    var boundFieldsDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(entity["fields"].ToString());

                    var jss = new JavaScriptSerializer();
                    string formData = jss.Serialize(SetFormData(formDataDict, user));
                    string boundFields = string.Join(",", boundFieldsDict.Keys.ToArray());

                    bool submitResult = InfoSubmit(ref httpClient, formData, sid, boundFields, csrfToken);
                    if (submitResult == false)
                    {
                        throw new Exception("打卡失败");
                    }
                }
                catch (Exception ex)
                {
                    // TODO
                    throw ex;
                }
            }
            return 0;

        }

        static void StudentSignIn(ref Dictionary<object, object> user, ref HttpClient httpClient)
        {
            //string ClientURL = "https://ehall.jlu.edu.cn/";
            //HttpClient httpClient = InitHttpClient(ClientURL);
            string ApplyLoginURL = "https://ehall.jlu.edu.cn/jlu_portal/login";
            HttpResponseMessage response = httpClient.GetAsync(new Uri(ApplyLoginURL)).Result;
            string ApplyLoginResult = response.Content.ReadAsStringAsync().Result;
            // Console.Write(result);
            string pid = new Regex("(?<=name=\"pid\" value=\").{1,20}(?=(\"))").Match(ApplyLoginResult).Groups[0].Value;
            //Console.WriteLine("PID:{0}", pid);

            // 登录提交
            string username = user["username"].ToString();
            string password = user["password"].ToString();
            string LoginURL = "https://ehall.jlu.edu.cn/sso/login";
            var postPayLoad = new List<KeyValuePair<string, string>>();
            postPayLoad.Add(new KeyValuePair<string, string>("username", username));
            postPayLoad.Add(new KeyValuePair<string, string>("password", password));
            response = httpClient.PostAsync(new Uri(LoginURL), new FormUrlEncodedContent(postPayLoad)).Result;
            string LoginResult = response.Content.ReadAsStringAsync().Result;
            string LoginError = new Regex("(?<=class=\"for-form errorerror\" value=\").{1,30}(?=(\"))").Match(LoginResult).Groups[0].Value;
            //Console.Write(LoginResult);
            //Console.WriteLine(LoginError);
            if (LoginError == "INVALID_PASSWORD")
            {
                throw new Exception("账号或密码错误");
            }
        }

        static string GetCSRFToken(ref HttpClient httpClient, string transaction)
        {
            string RequestURL = "https://ehall.jlu.edu.cn/infoplus/form/" + transaction + "/start";
            HttpResponseMessage response = httpClient.GetAsync(new Uri(RequestURL)).Result;
            string RequestResult = response.Content.ReadAsStringAsync().Result;
            //Console.Write(RequestResult);
            string csrfToken = new Regex("(?<=itemscope=\"csrfToken\" content=\").{1,200}(?=(\"))").Match(RequestResult).Groups[0].Value;
            //Console.WriteLine("CSRFToken:{0}", csrfToken);
            return csrfToken;
        }

        static string Start(ref HttpClient httpClient, string transaction, string csrfToken)
        {
            /* return sid */
            string StartURL = "https://ehall.jlu.edu.cn/infoplus/interface/start";
            var postPayLoad = new List<KeyValuePair<string, string>>();
            postPayLoad.Add(new KeyValuePair<string, string>("idc", transaction));
            postPayLoad.Add(new KeyValuePair<string, string>("csrfToken", csrfToken));
            HttpResponseMessage response = httpClient.PostAsync(new Uri(StartURL), new FormUrlEncodedContent(postPayLoad)).Result;
            string StartResult = response.Content.ReadAsStringAsync().Result;
            //Console.Write(StartResult);
            string errno = new Regex("(?<=errno\":).{1,10}(?=,)").Match(StartResult).Groups[0].Value;
            if (errno == "22001")
            {
                throw new Exception("当前不是打卡时间");
            }
            string sid = new Regex("(?<=form/)\\d*(?=/render)").Match(StartResult).Groups[0].Value;
            //Console.WriteLine("Step ID:{0}", sid);
            return sid;
        }

        static Dictionary<object, object> GetEntityByRender(ref HttpClient httpClient, string sid, string csrfToken)
        {
            /* return the first entity og entities */
            string RenderURL = "https://ehall.jlu.edu.cn/infoplus/interface/render";
            var postPayLoad = new List<KeyValuePair<string, string>>();
            postPayLoad.Add(new KeyValuePair<string, string>("stepId", sid));
            postPayLoad.Add(new KeyValuePair<string, string>("csrfToken", csrfToken));
            HttpResponseMessage response = httpClient.PostAsync(new Uri(RenderURL), new FormUrlEncodedContent(postPayLoad)).Result;
            string RenderResult = response.Content.ReadAsStringAsync().Result;

            var dict = JsonConvert.DeserializeObject<Dictionary<object, object>>(RenderResult);
            var arr = dict["entities"] as JArray;
            var data = JsonConvert.DeserializeObject<Dictionary<object, object>>(arr[0].ToString());
            return data;
        }

        static Dictionary<object, object> SetFormData(Dictionary<object, object> formDataDict, Dictionary<object, object> user)
        {
            formDataDict["fieldZtw"] = "1";
            if (formDataDict["fieldXY1"].ToString() == "1")
            {
                formDataDict["fieldZhongtw"] = "1";
            }
            if (formDataDict["fieldXY2"].ToString() == "1")
            {
                formDataDict["fieldWantw"] = "1";
            }
            var informs = JsonConvert.DeserializeObject<Dictionary<object, object>>(user["fields"].ToString());
            foreach (var inform in informs)
            {
                formDataDict[inform.Key.ToString()] = inform.Value.ToString();
            }
            return formDataDict;
        }

        static bool InfoSubmit(ref HttpClient httpClient, string formData, string sid, string boundFields, string csrfToken)
        {
            var postPayLoad = new List<KeyValuePair<string, string>>();
            postPayLoad.Add(new KeyValuePair<string, string>("actionId", "1"));
            postPayLoad.Add(new KeyValuePair<string, string>("formData", formData));
            postPayLoad.Add(new KeyValuePair<string, string>("nextUsers", "{}"));
            postPayLoad.Add(new KeyValuePair<string, string>("stepId", sid));
            DateTime d = new DateTime(1970, 1, 1, 8, 0, 0);
            string timeStamp = Convert.ToInt64((DateTime.Now - d).TotalSeconds).ToString();
            postPayLoad.Add(new KeyValuePair<string, string>("timestamp", timeStamp));
            postPayLoad.Add(new KeyValuePair<string, string>("boundFields", boundFields));
            postPayLoad.Add(new KeyValuePair<string, string>("csrfToken", csrfToken));

            HttpResponseMessage response = httpClient.PostAsync(new Uri("https://ehall.jlu.edu.cn/infoplus/interface/doAction"), new FormUrlEncodedContent(postPayLoad)).Result;
            string SubmitResult = response.Content.ReadAsStringAsync().Result;
            //Console.Write(result);
            
            var SubmitResultDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(SubmitResult);
            if (SubmitResultDict["ecode"].ToString() != "SUCCEED")
            {
                return false;
            }
            return true;
        }

        static HttpClient InitHttpClient(KeyValuePair<string, string> RequestHeaders)
        {
            HttpClient s = new HttpClient();
            s.MaxResponseContentBufferSize = 256000;
            s.DefaultRequestHeaders.Add(RequestHeaders.Key, RequestHeaders.Value);
            //s.DefaultRequestHeaders.Add("Referer", "https://ehall.jlu.edu.cn/");
            return s;
        }


        
    }
}
