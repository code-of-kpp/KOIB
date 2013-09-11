namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Результат выполнения команды Drop (сброса бюллетеня в урну) 

    /// </summary> 

    public enum DropResult 

    { 

        /// <summary> 

        /// Бюллетень сброшен в урну 

        /// </summary> 

        Dropped, 

        /// <summary> 

        /// Бюллетень был реверсирован 

        /// </summary> 

        Reversed, 

        /// <summary> 

        /// Не дождались выполнения команды 

        /// </summary> 

        Timeout, 

    } 

}


