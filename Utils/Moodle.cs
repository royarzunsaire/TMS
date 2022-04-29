using SGC.Models;
using SGC.Models.Feedback;
using SGC.Models.Moodle.Feedback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace SGC.Utils
{
    public class Moodle
    {
        public static string LoginMoodle(ParametrosMoodle parametrosMoodle)
        {
            if (parametrosMoodle == null)
            {
                return "exception";
            }

            string createRequest = string.Format(
                "{0}/login/token.php?"
                , parametrosMoodle.urlMoodle);

            // Call Moodle REST Service
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            var postData = String.Format("username={0}&password={1}&service={2}"
                , HttpUtility.UrlEncode(parametrosMoodle.username), HttpUtility.UrlEncode(parametrosMoodle.password)
                , HttpUtility.UrlEncode(parametrosMoodle.service));

            // Encode the parameters as form data:
            byte[] formData =
                UTF8Encoding.UTF8.GetBytes(postData);
            req.ContentLength = formData.Length;

            // Write out the form Data to the request:
            string contents = "";
            try
            {
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }

                // Get the Response
                try
                {
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    Stream resStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(resStream);
                    contents = reader.ReadToEnd();
                }
                catch (WebException e)
                {
                    contents = "exception: " + e.Message;
                }
            }
            catch (Exception e)
            {
                contents = "exception: " + e.Message;
            }

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error, bind error object
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                return contents;
                //return null;
            }
            else
            {
                // Good, bind object
                MoodleLoginResponse loginResp = serializer.Deserialize<MoodleLoginResponse>(contents);
                return loginResp.token;
            }
        }

        private static string PostMoodle(ParametrosMoodle parametrosMoodle, string funcionMoodle, string postData)
        {
            var token = LoginMoodle(parametrosMoodle);

            if (token.Contains("exception"))
            {
                return token;
            }

            string createRequest = string.Format(
                "{0}/webservice/rest/server.php?wstoken={1}&wsfunction={2}&moodlewsrestformat=json"
                , parametrosMoodle.urlMoodle, token, funcionMoodle);

            // Call Moodle REST Service
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            // Encode the parameters as form data:
            byte[] formData =
                UTF8Encoding.UTF8.GetBytes(postData);
            req.ContentLength = formData.Length;

            // Write out the form Data to the request:
            using (Stream post = req.GetRequestStream())
            {
                post.Write(formData, 0, formData.Length);
            }

            // Get the Response
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                return reader.ReadToEnd();
            }
            catch (WebException e)
            {
                return "exception";
            }
        }

        //public static string test(ParametrosMoodle parametrosMoodle)
        //{
        //    //var postData = String.Format("criteria[0][key]=id&criteria[0][value]=145");
        //    var postData = String.Format("courseids[0]=20");

        //    var contents = PostMoodle(parametrosMoodle, "mod_quiz_get_quizzes_by_courses ", postData);

        //    // Deserialize
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    if (contents.Contains("exception"))
        //    {
        //        // Error
        //        MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
        //    }
        //    else
        //    {
        //        // Good
        //        List<MoodleUser> newUsers = serializer.Deserialize<List<MoodleUser>>(contents);
        //    }
        //    return contents;
        //}

        // --------------------------------------------- Usuarios ------------------------------------------

        public static string ValidarSiUsuarioYaExiste(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("criteria[0][key]={0}&criteria[0][value]={1}", HttpUtility.UrlEncode("username"), HttpUtility.UrlEncode(contacto.run.ToLower()));

            var contents = PostMoodle(parametrosMoodle, "core_user_get_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al verificar si el usuario ya se encuentra ingresado a la plataforma Moodle";
            }
            else
            {
                // Good
                MoodleUsersSearchResponse newUsers = serializer.Deserialize<MoodleUsersSearchResponse>(contents);
                if (newUsers.users.Count() == 0)
                {
                    return "";
                }
                return "El usuario " + contacto.run.ToLower() + " ya existe en la plataforma Moodle";
            }
        }

        public static string idUsuarioExistente(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("criteria[0][key]={0}&criteria[0][value]={1}", HttpUtility.UrlEncode("username"), HttpUtility.UrlEncode(contacto.run.ToLower()));

            var contents = PostMoodle(parametrosMoodle, "core_user_get_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "";
            }
            else
            {
                // Good
                MoodleUsersSearchResponse newUsers = serializer.Deserialize<MoodleUsersSearchResponse>(contents);
                if (newUsers.users.Count() == 0)
                {
                    return "";
                }
                return newUsers.users.FirstOrDefault().id;
            }
        }

        public static string ValidarSiEmailYaExiste(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("criteria[0][key]={0}&criteria[0][value]={1}", HttpUtility.UrlEncode("email"), HttpUtility.UrlEncode(contacto.correo));

            var contents = PostMoodle(parametrosMoodle, "core_user_get_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al verificar si el correo electrónico del participante ya existe en la plataforma Moodle";
            }
            else
            {
                // Good
                MoodleUsersSearchResponse newUsers = serializer.Deserialize<MoodleUsersSearchResponse>(contents);
                if (newUsers.users.Count() == 0)
                {
                    return "";
                }
                return "El correo electrónico " + contacto.correo + " ya existe en la plataforma Moodle";
            }
        }

        public static string CrearUsuarioMoodle(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            MoodleUser user = new MoodleUser();
            user.username = HttpUtility.UrlEncode(contacto.run.ToLower());
            user.password = HttpUtility.UrlEncode(parametrosMoodle.contraseñaUsuarios);
            user.firstname = HttpUtility.UrlEncode(contacto.nombres);
            user.lastname = HttpUtility.UrlEncode(String.Format("{0} {1}", contacto.apellidoPaterno, contacto.apellidoMaterno));
            user.email = HttpUtility.UrlEncode(contacto.correo);
            user.city = HttpUtility.UrlEncode("Antofagasta");
            user.country = HttpUtility.UrlEncode("CL");
            user.idnumber = HttpUtility.UrlEncode(contacto.run.Replace(".", ""));

            List<MoodleUser> userList = new List<MoodleUser>();
            userList.Add(user);

            Array arrUsers = userList.ToArray();

            String postData = String.Format(
                "users[0][username]={0}&users[0][password]={1}&users[0][firstname]={2}&users[0][lastname]={3}&users[0][email]={4}&users[0][city]={5}&users[0][country]={6}&users[0][idnumber]={7}"
                , user.username, user.password, user.firstname, user.lastname, user.email, user.city, user.country, user.idnumber);

            var contents = PostMoodle(parametrosMoodle, "core_user_create_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el participante a la plataforma Moodle";
            }
            else
            {
                // Good
                List<MoodleUser> newUsers = serializer.Deserialize<List<MoodleUser>>(contents);
                return newUsers.FirstOrDefault().id;
            }
        }

        public static string CrearAllUsuarioMoodle(List<Participante> participantes, ParametrosMoodle parametrosMoodle)
        {
            String postData = "";
            int i = 0;
            foreach (Contacto contacto in participantes.Select(x => x.contacto))
            {
                MoodleUser user = new MoodleUser();

                user.username = HttpUtility.UrlEncode(contacto.run.ToLower());
                user.password = HttpUtility.UrlEncode(parametrosMoodle.contraseñaUsuarios);
                user.firstname = HttpUtility.UrlEncode(contacto.nombres);
                user.lastname = HttpUtility.UrlEncode(String.Format("{0} {1}", contacto.apellidoPaterno, contacto.apellidoMaterno));
                user.email = HttpUtility.UrlEncode(contacto.correo);
                user.idnumber = HttpUtility.UrlEncode(contacto.run.Replace(".", ""));

                postData += String.Format(
                    "users[{9}][username]={1}&users[{9}][password]={2}&users[{9}][firstname]={3}&users[{9}][lastname]={4}&users[{9}][email]={5}&users[{9}][city]={6}&users[{9}][country]={7}&users[{9}][idnumber]={8}"
                   , user.username, user.password, user.firstname, user.lastname, user.email, user.city, user.country, user.idnumber, i);
                int total = i + 1;
                if (total != participantes.Select(x => x.contacto).Count())
                {
                    postData += "&";
                }
                i++;
            }



            var contents = PostMoodle(parametrosMoodle, "core_user_create_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el participante a la plataforma Moodle";
            }
            else
            {
                // Good
                return "Updated";
            }
        }



        public static string UpdateUsuarioMoodle(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            MoodleUser user = new MoodleUser();
            user.id = HttpUtility.UrlEncode(contacto.idUsuarioMoodle);
            user.username = HttpUtility.UrlEncode(contacto.run.ToLower());
            user.password = HttpUtility.UrlEncode(parametrosMoodle.contraseñaUsuarios);
            user.firstname = HttpUtility.UrlEncode(contacto.nombres);
            user.lastname = HttpUtility.UrlEncode(String.Format("{0} {1}", contacto.apellidoPaterno, contacto.apellidoMaterno));
            user.email = HttpUtility.UrlEncode(contacto.correo);
            user.idnumber = HttpUtility.UrlEncode(contacto.run.Replace(".", ""));

            List<MoodleUser> userList = new List<MoodleUser>();
            userList.Add(user);

            Array arrUsers = userList.ToArray();

            String postData = String.Format(
                "users[0][id]={0}&users[0][username]={1}&users[0][password]={2}&users[0][firstname]={3}&users[0][lastname]={4}&users[0][email]={5}&users[0][city]={6}&users[0][country]={7}&users[0][idnumber]={8}"
               , user.id, user.username, user.password, user.firstname, user.lastname, user.email, user.city, user.country, user.idnumber);

            var contents = PostMoodle(parametrosMoodle, "core_user_update_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el participante a la plataforma Moodle";
            }
            else
            {
                // Good
                return "Updated";
            }
        }
        public static string UpdateAllUsuarioMoodle(List<Participante> participantes, ParametrosMoodle parametrosMoodle)
        {
            String postData = "";
            int i = 0;
            foreach (Contacto contacto in participantes.Select(x => x.contacto))
            {
                MoodleUser user = new MoodleUser();
                user.id = HttpUtility.UrlEncode(contacto.idUsuarioMoodle);
                user.username = HttpUtility.UrlEncode(contacto.run.ToLower());
                user.password = HttpUtility.UrlEncode(parametrosMoodle.contraseñaUsuarios);
                user.firstname = HttpUtility.UrlEncode(contacto.nombres);
                user.lastname = HttpUtility.UrlEncode(String.Format("{0} {1}", contacto.apellidoPaterno, contacto.apellidoMaterno));
                user.email = HttpUtility.UrlEncode(contacto.correo);
                user.idnumber = HttpUtility.UrlEncode(contacto.run.Replace(".", ""));

                postData += String.Format(
                    "users[{9}][id]={0}&users[{9}][username]={1}&users[{9}][password]={2}&users[{9}][firstname]={3}&users[{9}][lastname]={4}&users[{9}][email]={5}&users[{9}][city]={6}&users[{9}][country]={7}&users[{9}][idnumber]={8}"
                   , user.id, user.username, user.password, user.firstname, user.lastname, user.email, user.city, user.country, user.idnumber, i);
                int total = i + 1;
                if (total != participantes.Select(x => x.contacto).Count())
                {
                    postData += "&";
                }
                i++;
            }


            var contents = PostMoodle(parametrosMoodle, "core_user_update_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el participante a la plataforma Moodle";
            }
            else
            {
                // Good
                return "Updated";
            }
        }
        //public static MoodleUserSearchResponse GetUsuarioMoodle(Contacto contacto, ParametrosMoodle parametrosMoodle)
        //{
        //    var postData = String.Format("criteria[0][key]={0}&criteria[0][value]={1}", HttpUtility.UrlEncode("username"), HttpUtility.UrlEncode(contacto.run.ToLower()));

        //    var contents = PostMoodle(parametrosMoodle, "core_user_get_users", postData);

        //    // Deserialize
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    if (contents.Contains("exception"))
        //    {
        //        // Error
        //        MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
        //        return null;
        //        //return "Se produjo un error al ingresar el participante a la plataforma Moodle";
        //    }
        //    else
        //    {
        //        // Good
        //        MoodleUsersSearchResponse newUsers = serializer.Deserialize<MoodleUsersSearchResponse>(contents);
        //        if (newUsers.users.Count() != 0)
        //        {
        //            return newUsers.users.FirstOrDefault();
        //        }
        //        return null;
        //    }
        //}

        public static void EliminarUsuarioMoodle(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            //MoodleUserSearchResponse user = GetUsuarioMoodle(contacto, parametrosMoodle);

            var postData = String.Format("userids[0]={0}", HttpUtility.UrlEncode(contacto.idUsuarioMoodle));

            var contents = PostMoodle(parametrosMoodle, "core_user_delete_users", postData);

            //// Deserialize
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //if (contents.Contains("exception"))
            //{
            //    // Error
            //    MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
            //    //return "Se produjo un error al remover el participante de la plataforma Moodle";
            //}
            //else
            //{
            //    // Good
            //    List<MoodleUser> newUsers = serializer.Deserialize<List<MoodleUser>>(contents);
            //}
        }

        public static string EditarUsuarioMoodle(Contacto contacto, ParametrosMoodle parametrosMoodle)
        {
            //MoodleUserSearchResponse userMoodle = GetUsuarioMoodle(contacto, parametrosMoodle);

            MoodleUser user = new MoodleUser();
            user.id = HttpUtility.UrlEncode(contacto.idUsuarioMoodle);
            user.firstname = HttpUtility.UrlEncode(contacto.nombres);
            user.lastname = HttpUtility.UrlEncode(contacto.apellidoPaterno + " " + contacto.apellidoMaterno);
            user.email = HttpUtility.UrlEncode(contacto.correo);

            List<MoodleUser> userList = new List<MoodleUser>();
            userList.Add(user);

            Array arrUsers = userList.ToArray();

            String postData = String.Format(
                "users[0][id]={0}&users[0][firstname]={1}&users[0][lastname]={2}&users[0][email]={3}"
                , user.id, user.firstname, user.lastname, user.email);

            var contents = PostMoodle(parametrosMoodle, "core_user_update_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al actualizar la información del participante a la plataforma Moodle";
            }
            else
            {
                // Good
                List<MoodleUser> newUsers = serializer.Deserialize<List<MoodleUser>>(contents);
                return "";
            }
        }

        // ---------------------------------------- Cursos --------------------------------------

        private static MoodleCategorySearchResponse GetCategoryMoodle(CategoriaR11 categoria, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("criteria[0][key]={0}&criteria[0][value]={1}", HttpUtility.UrlEncode("idnumber"), HttpUtility.UrlEncode(categoria.identificador));

            var contents = PostMoodle(parametrosMoodle, "core_course_get_categories", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                return null;
                //return "Se produjo un error al ingresar el participante a la plataforma Moodle";
            }
            else
            {
                // Good
                List<MoodleCategorySearchResponse> categories = serializer.Deserialize<List<MoodleCategorySearchResponse>>(contents);
                if (categories.Count() != 0)
                {
                    return categories.FirstOrDefault();
                }
                return null;
            }
        }

        public static string GetIdCursoMoodle(Curso curso, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("field={0}&value={1}", HttpUtility.UrlEncode("idnumber"), HttpUtility.UrlEncode(curso.idCursoMoodle));

            var contents = PostMoodle(parametrosMoodle, "core_course_get_courses_by_field", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                return null;
                //return "Se produjo un error al ingresar el participante a la plataforma Moodle";
            }
            else
            {
                // Good
                MoodleCourses courses = serializer.Deserialize<MoodleCourses>(contents);
                if (courses.courses.Count() == 1 && courses.courses.FirstOrDefault().idnumber == curso.idCursoMoodle)
                {
                    return courses.courses.FirstOrDefault().id;
                }
                return null;
            }
        }

        public static bool IsInCourseGroup(string idCourse, string nameGroup, ParametrosMoodle parametrosMoodle)
        {
            bool isCourse = false;
            Grupo result = FindGroupByNameAndCourse(idCourse, nameGroup, parametrosMoodle);
            if (result != null)
            {
                isCourse = true;
            }

            return isCourse;
        }
        public static Grupo FindGroupByNameAndCourse(string idCourse, string nameGroup, ParametrosMoodle parametrosMoodle)
        {
            List<Grupo> grupos = GetAllGroupByCourseMoodle(idCourse, parametrosMoodle);
            Grupo result = null;
            if (grupos != null)
                result = grupos.FirstOrDefault(x => x.name == nameGroup);
            return result;
        }
        private static List<Grupo> GetAllGroupByCourseMoodle(string idCourse, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("courseid={0}", HttpUtility.UrlEncode(idCourse));
            //var postData = String.Format("courseid={0}", "AB");
            var contents = PostMoodle(parametrosMoodle, "core_group_get_course_groups", postData);
            List<Grupo> grupos = null;
            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (!contents.Contains("exception"))
            {
                grupos = serializer.Deserialize<List<Grupo>>(contents);
            }

            return grupos;
        }


        public static string CrearCursoMoodle(Curso curso, R51 r51, CategoriaR11 categoria, ParametrosMoodle parametrosMoodle)
        {
            MoodleCategorySearchResponse categoryMoodle = GetCategoryMoodle(categoria, parametrosMoodle);

            if (categoryMoodle == null)
            {
                return "No se encontró la categoría seleccionada en la plataforma Moodle";
            }

            var nombreCurso = r51.nombreCurso;
            if (curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                || curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica)
            {
                nombreCurso = "(S) " + nombreCurso;
            }
            if (curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
            {
                nombreCurso = "(A) " + nombreCurso;
            }

            MoodleCourse course = new MoodleCourse();
            course.fullname = HttpUtility.UrlEncode(nombreCurso);
            course.shortname = HttpUtility.UrlEncode(nombreCurso);
            course.categoryid = HttpUtility.UrlEncode(categoryMoodle.id);
            course.idnumber = HttpUtility.UrlEncode(r51.idR51.ToString());

            List<MoodleCourse> courseList = new List<MoodleCourse>();
            courseList.Add(course);

            Array arrCourses = courseList.ToArray();

            String postData = String.Format(
                "courses[0][fullname]={0}&courses[0][shortname]={1}&courses[0][categoryid]={2}&courses[0][idnumber]={3}"
                , course.fullname, course.shortname, course.categoryid, course.idnumber);

            var contents = PostMoodle(parametrosMoodle, "core_course_create_courses", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el curso a la plataforma Moodle";
            }
            else
            {
                // Good
                List<MoodleCourse> newCourse = serializer.Deserialize<List<MoodleCourse>>(contents);
                return course.idnumber;
            }
        }

        public static void EliminarCursoMoodle(Curso curso, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("courseids[0]={0}", HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle)));

            var contents = PostMoodle(parametrosMoodle, "core_course_delete_courses", postData);
        }

        public static string EditarCursoMoodle(Curso curso, R51 r51, CategoriaR11 categoria, ParametrosMoodle parametrosMoodle, string method)
        {
            MoodleCategorySearchResponse categoryMoodle = GetCategoryMoodle(categoria, parametrosMoodle);

            var nombreCurso = r51.nombreCurso;
            if (curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                || curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica)
            {
                nombreCurso = "(S) " + nombreCurso;
            }
            if (curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
            {
                nombreCurso = "(A) " + nombreCurso;
            }

            MoodleCourse course = new MoodleCourse();
            course.id = HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle));
            course.fullname = HttpUtility.UrlEncode(nombreCurso);
            course.shortname = HttpUtility.UrlEncode(nombreCurso);
            course.categoryid = HttpUtility.UrlEncode(categoryMoodle.id);
            course.idnumber = HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle));

            List<MoodleCourse> courseList = new List<MoodleCourse>();
            courseList.Add(course);

            Array arrCourses = courseList.ToArray();

            String postData = String.Format(
                "courses[0][id]={0}&courses[0][fullname]={1}&courses[0][shortname]={2}&courses[0][categoryid]={3}&courses[0][idnumber]={4}"
                , course.id, course.fullname, course.shortname, course.categoryid, course.idnumber);

            var contents = PostMoodle(parametrosMoodle, "core_course_update_courses", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al actualizar la información del curso a la plataforma Moodle";
            }
            else
            {
                // Good
                List<MoodleCourse> newCourse = serializer.Deserialize<List<MoodleCourse>>(contents);
                if (method == "Crear")
                {
                    return course.idnumber;
                }
                if (method == "Actualizar")
                {
                    return course.idnumber;
                }
                else
                {

                    return "";
                }

            }
        }
        public static string AgregarParticipantesCursoMoodle(List<Contacto> contactos, Curso curso, ParametrosMoodle parametrosMoodle, DateTime fechaInicio, DateTime fechaTermino)
        {
            var postData = "";
            int i = 0;
            string separador = "&";
            foreach (var contacto in contactos)
            {
                if (i + 1 == contactos.Count())
                {
                    separador = "";
                }
                postData += String.Format("enrolments[{5}][roleid]={0}&enrolments[{5}][userid]={1}&enrolments[{5}][courseid]={2}&enrolments[{5}][timestart]={3}&enrolments[{5}][timeend]={4}{6}"
                , HttpUtility.UrlEncode(parametrosMoodle.idRolEstudiante)
                , HttpUtility.UrlEncode(contacto.idUsuarioMoodle)
                , HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle))
                , HttpUtility.UrlEncode(fechaInicio.Date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString())
                , HttpUtility.UrlEncode(fechaTermino.AddDays(7).Date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString())
                , i, separador);
                i++;
            }

            var contents = PostMoodle(parametrosMoodle, "enrol_manual_enrol_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);

                return "Se produjo un error al ingresar el usuario al curso en la plataforma Moodle : " + moodleError.message;
            }

            // Good
            return null;

        }
        public static string EliminarParticipanteCursoMoodle(Contacto contacto, Curso curso, ParametrosMoodle parametrosMoodle)
        {
            var postData = "";



            postData += String.Format("enrolments[{3}][roleid]={0}&enrolments[{3}][userid]={1}&enrolments[{3}][courseid]={2}"
            , HttpUtility.UrlEncode(parametrosMoodle.idRolEstudiante)
            , HttpUtility.UrlEncode(contacto.idUsuarioMoodle)
            , HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle)),
            0
            );



            var contents = PostMoodle(parametrosMoodle, "enrol_manual_unenrol_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);

                return "Se produjo un error al Eliminar el usuario al curso en la plataforma Moodle : " + moodleError.message;
            }

            // Good
            return null;

        }
        public static string AgregarParticipanteCursoMoodle(Contacto contacto, Curso curso, ParametrosMoodle parametrosMoodle, DateTime fechaInicio, DateTime fechaTermino)
        {
            var postData = String.Format("enrolments[0][roleid]={0}&enrolments[0][userid]={1}&enrolments[0][courseid]={2}&enrolments[0][timestart]={3}&enrolments[0][timeend]={4}"
                , HttpUtility.UrlEncode(parametrosMoodle.idRolEstudiante)
                , HttpUtility.UrlEncode(contacto.idUsuarioMoodle)
                , HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle))
                , HttpUtility.UrlEncode(fechaInicio.Date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString())
                , HttpUtility.UrlEncode(fechaTermino.AddDays(7).Date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString()));

            var contents = PostMoodle(parametrosMoodle, "enrol_manual_enrol_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return contents;
                return "Se produjo un error al ingresar el usuario al curso en la plataforma Moodle: " + moodleError.message;
            }
            else
            {
                // Good
                return "";
            }
        }
        public static string AgregarParticipantesCursoMoodle(Contacto contacto, Curso curso, ParametrosMoodle parametrosMoodle, DateTime fechaInicio, DateTime fechaTermino)
        {
            var postData = String.Format("enrolments[0][roleid]={0}&enrolments[0][userid]={1}&enrolments[0][courseid]={2}&enrolments[0][timestart]={3}&enrolments[0][timeend]={4}"
                , HttpUtility.UrlEncode(parametrosMoodle.idRolEstudiante)
                , HttpUtility.UrlEncode(contacto.idUsuarioMoodle)
                , HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle))
                , HttpUtility.UrlEncode(fechaInicio.Date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString())
                , HttpUtility.UrlEncode(fechaTermino.AddDays(7).Date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString()));

            var contents = PostMoodle(parametrosMoodle, "enrol_manual_enrol_users", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return contents;
                return "Se produjo un error al ingresar el usuario al curso en la plataforma Moodle";
            }
            else
            {
                // Good
                return "";
            }
        }
        public static void RemoverParticipanteCursoMoodle(Contacto contacto, Curso curso, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("enrolments[0][userid]={0}&enrolments[0][courseid]={1}"
                , HttpUtility.UrlEncode(contacto.idUsuarioMoodle), HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle)));

            var contents = PostMoodle(parametrosMoodle, "enrol_manual_unenrol_users", postData);
        }

        public static List<MoodleQuiz> GetEvaluacionesCursoMoodle(Curso curso, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("courseids[0]={0}"
                , HttpUtility.UrlEncode(GetIdCursoMoodle(curso, parametrosMoodle)));

            var contents = PostMoodle(parametrosMoodle, "mod_quiz_get_quizzes_by_courses", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                return null;
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                //return "Se produjo un error al ingresar el usuario al grupo en la plataforma Moodle";
            }
            else
            {
                // Good
                MoodleSearchQuizzes evaluaciones = serializer.Deserialize<MoodleSearchQuizzes>(contents);
                return evaluaciones.quizzes;
            }
        }

        // ----------------------------------------------- Grupos ----------------------------------------

        public static string CrearGrupoMoodle(Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            Grupo exist = FindGroupByNameAndCourse(comercializacion.cotizacion.curso.idCursoMoodle, comercializacion.cotizacion.codigoCotizacion, parametrosMoodle);
            if (exist != null)
            {
                return exist.id;
            }
            MoodleGroup group = new MoodleGroup();
            group.courseid = HttpUtility.UrlEncode(GetIdCursoMoodle(comercializacion.cotizacion.curso, parametrosMoodle));
            group.name = HttpUtility.UrlEncode(comercializacion.cotizacion.codigoCotizacion);
            group.description = HttpUtility.UrlEncode(comercializacion.cotizacion.codigoCotizacion);
            group.idnumber = HttpUtility.UrlEncode(comercializacion.cotizacion.codigoCotizacion);

            List<MoodleGroup> groupList = new List<MoodleGroup>();
            groupList.Add(group);

            //Array arrGroups = groupList.ToArray();

            String postData = String.Format(
                "groups[0][courseid]={0}&groups[0][name]={1}&groups[0][description]={2}&groups[0][idnumber]={3}"
                , group.courseid, group.name, group.description, group.idnumber);

            var contents = PostMoodle(parametrosMoodle, "core_group_create_groups", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el grupo a la plataforma Moodle";
            }
            else
            {
                // Good
                List<MoodleGroup> newGroup = serializer.Deserialize<List<MoodleGroup>>(contents);
                return newGroup.FirstOrDefault().id;
            }
        }

        public static void EliminarGrupoMoodle(Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("groupids[0]={0}", HttpUtility.UrlEncode(comercializacion.idGrupoMoodle));

            var contents = PostMoodle(parametrosMoodle, "core_group_delete_groups", postData);

        }
        public static string AgregarParticipantesGrupoMoodle(List<Contacto> contactos, Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {


            if (comercializacion.idGrupoMoodle == null)
            {
                return "No se ha creado el grupo a moodle";
            }

            var agregarUsuarioCurso = AgregarParticipantesCursoMoodle(contactos, comercializacion.cotizacion.curso, parametrosMoodle, comercializacion.fechaCreacion, comercializacion.fechaTermino);

            if (agregarUsuarioCurso != null)
            {
                return agregarUsuarioCurso;
            }
            var postData = "";
            int i = 0;
            var separador = "&";
            foreach (var contacto in contactos)
            {
                if (i + 1 == contactos.Count())
                {
                    separador = "";
                }
                postData += String.Format("members[{2}][groupid]={0}&members[{2}][userid]={1}{3}"
                , HttpUtility.UrlEncode(comercializacion.idGrupoMoodle), HttpUtility.UrlEncode(contacto.idUsuarioMoodle)
                , i, separador);
                i++;
            }

            var contents = PostMoodle(parametrosMoodle, "core_group_add_group_members", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el usuario al grupo en la plataforma Moodle";
            }

            return null;
        }
        public static string AgregarParticipanteGrupoMoodle(Contacto contacto, Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            if (comercializacion.idGrupoMoodle == null)
            {
                return "No se ha creado el grupo a moodle";
            }
            var agregarUsuarioCurso = AgregarParticipanteCursoMoodle(contacto, comercializacion.cotizacion.curso, parametrosMoodle, comercializacion.fechaCreacion, comercializacion.fechaTermino);
            //var agregarUsua = UpdateParticipanteCursoMoodle(contacto, comercializacion.cotizacion.curso, parametrosMoodle, comercializacion.fechaInicio, comercializacion.fechaTermino);
            //agregarUsuarioCurso = AgregarParticipanteCursoMoodle(contacto, comercializacion.cotizacion.curso, parametrosMoodle, comercializacion.fechaInicio, comercializacion.fechaTermino);

            if (agregarUsuarioCurso != "")
            {
                return agregarUsuarioCurso;
            }
            var postData = String.Format("members[0][groupid]={0}&members[0][userid]={1}"
                , HttpUtility.UrlEncode(comercializacion.idGrupoMoodle), HttpUtility.UrlEncode(contacto.idUsuarioMoodle));

            var contents = PostMoodle(parametrosMoodle, "core_group_add_group_members", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                return "Se produjo un error al ingresar el usuario al grupo en la plataforma Moodle";
            }
            else
            {
                // Good
                return "";
            }
        }
        public static void RemoverParticipanteGrupoMoodle(Contacto contacto, Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("members[0][groupid]={0}&members[0][userid]={1}"
                , HttpUtility.UrlEncode(comercializacion.idGrupoMoodle), HttpUtility.UrlEncode(contacto.idUsuarioMoodle));

            var contents = PostMoodle(parametrosMoodle, "core_group_delete_group_members", postData);

            //RemoverParticipanteCursoMoodle(contacto, comercializacion.cotizacion.curso, parametrosMoodle);
        }

        public static MoodleSearchUserGrades GetNotasGrupoMoodle(Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            var postData = String.Format("courseid={0}&groupid={1}"
                , HttpUtility.UrlEncode(GetIdCursoMoodle(comercializacion.cotizacion.curso, parametrosMoodle)), HttpUtility.UrlEncode(comercializacion.idGrupoMoodle));

            var contents = PostMoodle(parametrosMoodle, "gradereport_user_get_grade_items", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (contents.Contains("exception"))
            {
                // Error
                //MoodleExceptionResponse moodleError = serializer.Deserialize<MoodleExceptionResponse>(contents);
                return null;
                //return moodleError.debuginfo + " - " + moodleError.errorcode + " - " + moodleError.exception + " - " + moodleError.message;
                //return "Se produjo un error al ingresar el usuario al grupo en la plataforma Moodle";
            }
            else
            {
                // Good
                MoodleSearchUserGrades notas = serializer.Deserialize<MoodleSearchUserGrades>(contents);

                return notas;
            }
        }

        public static FeedbackAnalysis GetFeedback(Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            FeedbackAnalysis feedbackAnalysis = null;
            var postData = String.Format("courseids[0]={0}"
                , HttpUtility.UrlEncode(GetIdCursoMoodle(comercializacion.cotizacion.curso, parametrosMoodle)), HttpUtility.UrlEncode(comercializacion.idGrupoMoodle));

            var contents = PostMoodle(parametrosMoodle, "mod_feedback_get_feedbacks_by_courses", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // Good
            Feedback feedback = serializer.Deserialize<Feedback>(contents);
            if (feedback.feedbacks != null && feedback.feedbacks.Count() > 0 && feedback.feedbacks.Any(x => x.name.ToLower().Contains("encuesta")))
            {
                var feedbackData = feedback.feedbacks.FirstOrDefault(x => x.name.ToLower().Contains("encuesta"));
                postData = String.Format("feedbackid={0}"
               , HttpUtility.UrlEncode(Convert.ToString(feedbackData.id)));

                contents = PostMoodle(parametrosMoodle, "mod_feedback_get_items", postData);
                // Deserialize
                FeedbackItem feedbackItem = serializer.Deserialize<FeedbackItem>(contents);

                if (feedbackItem.items.Count() > 0)
                {
                    var id = feedback.feedbacks.Where(x => x.name.ToLower().Contains("encuesta")).Select(y => y.id).FirstOrDefault();
                    postData = String.Format("feedbackid={0}&courseid={1}&groupid={2}"
                , feedbackData.id, HttpUtility.UrlEncode(GetIdCursoMoodle(comercializacion.cotizacion.curso, parametrosMoodle)), HttpUtility.UrlEncode(comercializacion.idGrupoMoodle));

                    contents = PostMoodle(parametrosMoodle, "mod_feedback_get_analysis", postData);

                    // Deserialize
                    feedbackAnalysis = serializer.Deserialize<FeedbackAnalysis>(contents);

                    var items = feedbackItem.items.OrderBy(x => x.position).Where(x => x.typ.Contains("label")).ToList();
                    var count = feedbackItem.items.Count();

                    try
                    {
                        foreach (FeedbackItemData feedItem in items)
                        {
                            int pos = feedbackItem.items.IndexOf(feedItem) + 1;
                            while (pos != count && feedbackItem.items.ElementAt(pos).typ.Contains("multichoicerated") && feedbackAnalysis.itemsdata != null && feedbackAnalysis.itemsdata.Count() > 0)
                            {
                                if (feedItem.presentation.ToLower().Contains("apoyo"))
                                {
                                    feedbackAnalysis.itemsdata.FirstOrDefault(x => x.item.id == feedbackItem.items.ElementAt(pos).id).item.label = TipoFeedbackItem.MaterialDeApoyo.ToString();

                                }
                                else if (feedItem.presentation.ToLower().Contains("relator"))
                                {
                                    feedbackAnalysis.itemsdata.FirstOrDefault(x => x.item.id == feedbackItem.items.ElementAt(pos).id).item.label = TipoFeedbackItem.Relator.ToString();
                                }
                                else if (feedItem.presentation.ToLower().Contains("plataforma"))
                                {
                                    feedbackAnalysis.itemsdata.FirstOrDefault(x => x.item.id == feedbackItem.items.ElementAt(pos).id).item.label = TipoFeedbackItem.Infraestructura.ToString();
                                }

                                pos++;
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            return feedbackAnalysis;
        }

        public static List<FeedbackResponsesData> GetQuizAprendizaje(Comercializacion comercializacion, ParametrosMoodle parametrosMoodle)
        {
            FeedbackResponses QuizAprendizaje = null;
            var postData = String.Format("courseids[0]={0}"
                , HttpUtility.UrlEncode(GetIdCursoMoodle(comercializacion.cotizacion.curso, parametrosMoodle)), HttpUtility.UrlEncode(comercializacion.idGrupoMoodle));

            var contents = PostMoodle(parametrosMoodle, "mod_feedback_get_feedbacks_by_courses", postData);

            // Deserialize
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // Good
            Feedback feedback = serializer.Deserialize<Feedback>(contents);

            if (feedback.feedbacks != null && feedback.feedbacks.Count() > 0 && feedback.feedbacks.Any(x => x.name.ToLower().Contains("quiz")))
            {
                var feedbackData = feedback.feedbacks.FirstOrDefault(x => x.name.ToLower().Contains("quiz"));
                postData = String.Format("feedbackid={0}", HttpUtility.UrlEncode(Convert.ToString(feedbackData.id)));

                contents = PostMoodle(parametrosMoodle, "mod_feedback_get_responses_analysis", postData);
                // Deserialize
                QuizAprendizaje = serializer.Deserialize<FeedbackResponses>(contents);
            }

            //QuizAprendizaje = QuizAprendizaje.attempts.Where(x => comercializacion.participantes.Where(y => y != null && y.contacto.idUsuarioMoodle != null 
            //&& y.contacto.idUsuarioMoodle.Contains(x.userid.ToString())).Any()).ToList();

            var QuizAprendizajes = QuizAprendizaje.attempts.Where(x => comercializacion.participantes.Where(y => y != null && y.contacto.idUsuarioMoodle != null
            && y.contacto.idUsuarioMoodle.Contains(x.userid.ToString())).Any()).ToList();

            return QuizAprendizajes;
        }
    }



    public class MoodleLoginResponse
    {
        public string token { get; set; }
        public string privatetoken { get; set; }
    }

    public class MoodleExceptionResponse
    {
        public string exception { get; set; }
        public string errorcode { get; set; }
        public string message { get; set; }
        public string debuginfo { get; set; }
    }

    public class MoodleUser
    {
        public string id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string idnumber { get; set; }

    }

    public class MoodleUsersSearchResponse
    {
        public List<MoodleUserSearchResponse> users { get; set; }
    }

    public class MoodleUserSearchResponse
    {
        public string id { get; set; }
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string department { get; set; }
        public string firstaccess { get; set; }
        public string lastaccess { get; set; }
        public string auth { get; set; }
        public string suspended { get; set; }
        public string confirmed { get; set; }
        public string lang { get; set; }
        public string theme { get; set; }
        public string timezone { get; set; }
        public string description { get; set; }
        public string descriptionformat { get; set; }
        public string country { get; set; }
        public string profileimageurlsmall { get; set; }
        public string profileimageurl { get; set; }
    }

    public class MoodleCourses
    {
        public List<MoodleCourse> courses { get; set; }
    }

    public class MoodleCourse
    {
        public string id { get; set; }
        public string fullname { get; set; }
        public string shortname { get; set; }
        public string categoryid { get; set; }
        public string idnumber { get; set; }
    }

    public class MoodleCategoriesSearchResponse
    {
        public MoodleCategorySearchResponse[] categories { get; set; }
    }

    public class MoodleCategorySearchResponse
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class MoodleGroup
    {
        public string id { get; set; }
        public string courseid { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string idnumber { get; set; }
    }

    public class MoodleSearchUserGrades
    {
        public List<MoodleSearchGrades> usergrades { get; set; }
    }

    public class MoodleSearchGrades
    {
        public string courseid { get; set; }
        public string userid { get; set; }
        public List<MoodleGrade> gradeitems { get; set; }
    }

    public class MoodleGrade
    {
        public string id { get; set; }
        public string itemname { get; set; }
        public string iteminstance { get; set; }
        public string gradeformatted { get; set; }
        public string percentageformatted { get; set; }

        public string gradedatesubmitted { get; set; }
        public string gradedategraded { get; set; }
    }

    public class MoodleSearchQuizzes
    {
        public List<MoodleQuiz> quizzes { get; set; }
    }

    public class MoodleQuiz
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}