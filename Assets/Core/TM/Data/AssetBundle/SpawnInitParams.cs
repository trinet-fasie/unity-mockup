﻿using System.Collections.Generic;
using TM.Types;
using TM.Data.ServerData;
using TM.Models.Data;

// ReSharper disable once CheckNamespace
namespace TM.Data
{
    /// <summary>
    /// Параметры инициализации для спавна объекта
    /// </summary>
    public class SpawnInitParams : IJsonSerializable
    {
        /// <summary>
        /// Тип объекта
        /// </summary>
        public int IdObject;

        /// <summary>
        /// Имя
        /// </summary>
        public string Name;

        /// <summary>
        /// Id локации
        /// </summary>
        public int IdLocation;

        /// <summary>
        /// Трансформ объекта
        /// </summary>
        public Dictionary<int, TransformDT> Transforms;

        /// <summary>
        /// Какой id присвоить (если 0, то id получает атоматически)
        /// </summary>
        public int IdInstance = 0;

        /// <summary>
        /// Какой id объекта на сервере (если 0, то на сервере объекта пока нет)
        /// </summary>
        public int IdServer = 0;

        /// <summary>
        /// Ссылка на родителя
        /// </summary>
        public int? ParentId;

    }

     
   
}
