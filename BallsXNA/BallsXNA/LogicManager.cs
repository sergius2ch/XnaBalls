﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BallsXNA
{

    /// <summary>
    /// Класс для шарика
    /// </summary>
    class Ball
    {
        public bool OnEdge = false;
        /// <summary>
        /// координаты шарика
        /// </summary>
        public float x, y;

        /// <summary>
        /// Проекции скорости шарика на оси координат
        /// </summary>
        public float vx, vy;

        /// <summary>
        /// Конструктор шарика
        /// </summary>
        public Ball(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// Класс-менеджер для управления шариками
    /// </summary>
    class Manager
    {
        /// <summary>
        /// Массив шариков
        /// </summary>
        Ball[] balls = null;
        /// <summary>
        /// Количество шариков
        /// </summary>
        int NumberOfBalls;
        int Diameter;
        int Radius;
        /// <summary>
        /// Поле для шариков (прямоугольное)
        /// </summary>
        Rectangle field;

        /// <summary>
        /// Конструктор менеджера
        /// </summary>
        /// <param name="NumberOfBalls">Кол-во шариков</param>
        /// <param name="Diameter">Диаметр</param>
        /// <param name="field">Ограничивающий приямоугольник</param>
        public Manager(int NumberOfBalls, int Diameter, Rectangle field)
        {
            this.NumberOfBalls = NumberOfBalls;
            balls = new Ball[NumberOfBalls];
            this.Diameter = Diameter;
            Radius = Diameter / 2;
            this.field = field;
            InitBalls();
        }
        /// <summary>
        /// Инициализация шариков
        /// </summary>
        protected void InitBalls()
        {
            float x, y, V, angle;
            Random rand = new Random();
            #region Создадим сетку для расстановки шариков
            int columns = field.Width / Diameter;
            int rows = field.Height / Diameter;
            List<int> listX = new List<int>(columns * rows);
            List<int> listY = new List<int>(columns * rows);
            x = field.Left + Radius;
            y = field.Top + Radius;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    listX.Add((int)x);
                    listY.Add((int)y);
                    x += Diameter;
                }
                #region Переход на начало новой строки
                y += Diameter;
                x = field.Left + Radius;
                #endregion
            }
            #endregion

            #region Расставляем шарики на поле
            int index = 0;
            for (int i = 0; i < NumberOfBalls; i++)
            {
                // Выбираем случайную ячейку в сетке
                index = rand.Next(listX.Count);
                x = listX[index];
                y = listY[index];
                balls[i] = new Ball(x, y);
                listX.RemoveAt(index);
                listY.RemoveAt(index);
                // Возьмём случайную величину скорости 0..1
                V = (float)rand.NextDouble();
                // Возьмём случайную величину - направление движения
                angle = (float)(rand.NextDouble() * 2 * Math.PI);
                // Вычисляем проекции скорости на оси координат
                balls[i].vx = (float)(V * Math.Cos(angle));
                balls[i].vy = (float)(V * Math.Sin(angle));
            }
            #endregion
        }

        /// <summary>
        /// Обновление системы
        /// </summary>
        public void Update()
        {
            MoveBalls();
            CheckBorders();
            CheckCollisions();
        }

        /// <summary>
        /// Перемещаем все шарики
        /// </summary>
        protected void MoveBalls()
        {
            for (int i = 0; i < NumberOfBalls; i++)
            {
                balls[i].x += balls[i].vx;
                balls[i].y += balls[i].vy;
            }
        }

        /// <summary>
        /// Проверка на столкновения с границами
        /// </summary>
        protected void CheckBorders()
        {
            for (int i = 0; i < NumberOfBalls; i++)
            {
                balls[i].OnEdge = false;
                if (balls[i].x - Radius <= field.Left)
                { // шарик отскакивает от левой границы
                    balls[i].vx = -balls[i].vx;
                    if (balls[i].x - Radius < field.Left)
                    {
                        float dx = field.Left - (balls[i].x - Radius);
                        balls[i].x += dx+1;
                    }
                    balls[i].OnEdge = true;
                }
                if (balls[i].x + Radius >= field.Right)
                { // шарик отскакивает от правой границы
                    balls[i].vx = -balls[i].vx;
                    if (balls[i].x + Radius > field.Right)
                    {
                        float dx = (balls[i].x + Radius) - field.Right;
                        balls[i].x -= dx+1;
                    }
                    if (balls[i].OnEdge) return;
                    balls[i].OnEdge = true;
                } 
                if (balls[i].y - Radius <= field.Top)
                { // шарик отскакивает от верхней границы
                    balls[i].vy = -balls[i].vy;
                    if (balls[i].y - Radius < field.Top)
                    {
                        float dy = field.Top - (balls[i].y - Radius);
                        balls[i].y += dy+1;
                    }
                    if (balls[i].OnEdge) return;
                    balls[i].OnEdge = true;
                } 
                if (balls[i].y + Radius >= field.Bottom)
                { // шарик отскакивает от нижней границы
                    balls[i].vy = -balls[i].vy;
                    if (balls[i].y + Radius > field.Bottom)
                    {
                        float dy = (balls[i].y + Radius) - field.Bottom;
                        balls[i].y -= dy+1;
                    }
                    balls[i].OnEdge = true;
                }
            }
        }

        /// <summary>
        /// Проверка столкновений шариков др/др
        /// </summary>
        protected void CheckCollisions()
        {
            // пробегаем по всем шарикам, кроме последнего
            for (int i = 0; i < NumberOfBalls; i++)
            {
                // сравниваем со всеми последующими
                for (int j = i + 1; j < NumberOfBalls; j++)
                {
                    float dx = (int)(balls[i].x - balls[j].x);
                    float dy = (int)(balls[i].y - balls[j].y);
                    if (dx < Diameter && dy < Diameter)
                    {
                        float distance = (float)(Math.Sqrt(dx * dx + dy * dy));
                        if (distance < Diameter - 1)
                        { // шарики столкнулись
                            // Честная физика столкновений:
                            #region 1) Замена переменных для скоростей
                            float vx1 = balls[i].vx;
                            float vx2 = balls[j].vx;
                            float vy1 = balls[i].vy;
                            float vy2 = balls[j].vy;
                            #endregion
                            #region 2) Вычиляем единичный вектор столкновения
                            float ex = (dx / distance);
                            float ey = (dy / distance);
                            #endregion
                            #region 3) Проецируем вектора скоростей шариков на вектор столкновения
                            // первый шарик
                            float vex1 = (vx1 * ex + vy1 * ey);
                            float vey1 = (-vx1 * ey + vy1 * ex);
                            // второй шарик
                            float vex2 = (vx2 * ex + vy2 * ey);
                            float vey2 = (-vx2 * ey + vy2 * ex);
                            #endregion
                            #region 4) Вычисляем скорости после столкновения в проекции на вектор столкновения
                            float vPex = vex1 + (vex2 - vex1);
                            float vPey = vex2 + (vex1 - vex2);
                            #endregion
                            #region 5) Отменяем проецирование
                            vx1 = vPex * ex - vey1 * ey;
                            vy1 = vPex * ey + vey1 * ex;
                            vx2 = vPey * ex - vey2 * ey;
                            vy2 = vPey * ey + vey2 * ex;
                            #endregion
                            #region 6) Укажем шарикам их новые скорости
                            balls[i].vx = vx1;
                            balls[i].vy = vy1;
                            balls[j].vx = vx2;
                            balls[j].vy = vy2;
                            #endregion
                            #region 7) Устраним эффект залипания
                            if (distance < Diameter - 2)
                            {
                                if (!balls[i].OnEdge)
                                {
                                    balls[i].x += ex;
                                    balls[i].y += ey;
                                }
                                if (!balls[j].OnEdge)
                                {
                                    balls[j].x -= ex;
                                    balls[j].y -= ey;
                                }
                            }
                            #endregion
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Прорисовка шариков
        /// </summary>
        public void Draw(SpriteBatch batch, Texture2D tex)
        {
            
            foreach (Ball ball in balls)
            {
                batch.Draw(tex, 
                    new Vector2(ball.x - Radius, ball.y - Radius),
                    Color.White);
 
            }
        }
    }
}
