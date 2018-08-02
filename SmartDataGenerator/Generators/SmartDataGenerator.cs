﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SmartDataGenerator.Models;

namespace SmartDataGenerator.Generators
{
    public class SmartDataGenerator<T> where T : class, new()
    {
        private readonly int _total;
        private readonly Dictionary<string, Settings> _settings;
        private Dictionary<DataTypes, IGenerator> _generators;
        private PropertyInfo[] _properties;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        public SmartDataGenerator(int total)
        {
            if (total < 1 || total > 1000)
            {
                throw new ArgumentException("Total must be between 1 and 1000");
            }
            _total = total;
            _settings = new Dictionary<string, Settings>();
            _generators = new Dictionary<DataTypes, IGenerator>();
            _properties = typeof(T).GetProperties();
            foreach (var propertyInfo in _properties)
            {
                _settings.Add(propertyInfo.Name, new Settings()
                {
                    GenerationStrategy = GenerationStrategy.Random,
                    DataType = DataTypes.None
                });
            }
        }

        public SmartDataGenerator<T> Set<U>(Expression<Func<T,U>> expression, DataTypes type)
        {
            MemberExpression body = expression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException();
            }
            var propertyInfo = (PropertyInfo)body.Member;

            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;


            var setting = new Settings()
            {
                DataType = type,
                GenerationStrategy = GenerationStrategy.Random
            };
            _settings[propertyName] = setting;

            if (!_generators.ContainsKey(setting.DataType))
            {
                _generators[setting.DataType] = GeneratorFactory.GetGenerator(setting.DataType);
            }

            setting.Generator = _generators[setting.DataType];

            return this;
        }

        public T[] Generate()
        {
            var data = new T[_total];
            for (int i = 0; i < _total; i++)
            {
                data[i] = new T();
                foreach (var propertyInfo in _properties)
                {
                    var setting = _settings[propertyInfo.Name];
                    propertyInfo.SetValue(data[i], setting.Generator.Generate());
                }
            }

            return data;
        }
    }
}