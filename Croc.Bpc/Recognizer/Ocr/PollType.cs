using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Типы голосования 

    /// </summary> 

    public enum PollType 

    { 

        /// <summary> 

        /// oднoмандатные выбoры 

        /// </summary> 

        Single = 1, 

        /// <summary> 

        /// мнoгoмандатные выбoры 

        /// </summary> 

        Multi = 2, 

        /// <summary> 

        /// референдум 

        /// </summary> 

        Referendum = 4, 

        /// <summary> 

        /// опрос с телефоном и т.д. 

        /// </summary> 

        Question = 8 

    } 

}


