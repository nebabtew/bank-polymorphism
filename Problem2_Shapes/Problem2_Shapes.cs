using System;
using System.Collections.Generic;
using System.Globalization;

namespace Problem2_Shapes
{
    /// <summary>
    /// Thrown when a shape is constructed with a dimension that is zero or negative.
    /// </summary>
    public class InvalidShapeException : Exception
    {
        /// <summary>
        /// Creates a new InvalidShapeException.
        /// </summary>
        /// <param name="message">A description of the error.</param>
        public InvalidShapeException(string message) : base(message) { }
    }

    /// <summary>
    /// Allows a shape's dimensions to be resized by a scale factor.
    /// </summary>
    public interface IScalable
    {
        /// <summary>
        /// Scales the shape's dimensions by the given factor.
        /// </summary>
        /// <param name="factor">The multiplier applied to each dimension.</param>
        void Scale(double factor);
    }

    /// <summary>
    /// Base class for two-dimensional geometric shapes.
    /// </summary>
    public abstract class Shape
    {
        /// <summary>Calculates the area of the shape.</summary>
        public abstract double Area();

        /// <summary>Calculates the perimeter (or circumference) of the shape.</summary>
        public abstract double Perimeter();

        /// <summary>
        /// Returns a default text summary of the shape. Overridden by each subclass to
        /// produce its own report line, which is what makes this a useful demo of virtual dispatch.
        /// </summary>
        public virtual string Describe()
        {
            return $"{GetType().Name}: Area={Area():F2}, Perimeter={Perimeter():F2}";
        }
    }

    /// <summary>A circle defined by its radius.</summary>
    public class Circle : Shape, IScalable
    {
        private double radius;

        /// <summary>
        /// Creates a circle with the given radius.
        /// </summary>
        /// <param name="radius">The radius. Must be greater than zero.</param>
        /// <exception cref="InvalidShapeException">Thrown when radius is zero or negative.</exception>
        public Circle(double radius)
        {
            if (radius <= 0)
            {
                throw new InvalidShapeException("Circle radius must be greater than zero.");
            }
            this.radius = radius;
        }

        /// <inheritdoc/>
        public override double Area() => Math.PI * radius * radius;

        /// <inheritdoc/>
        public override double Perimeter() => 2 * Math.PI * radius;

        /// <inheritdoc/>
        public override string Describe()
        {
            return $"Circle | r={radius:F2} | Area: {Area():F2} | Perimeter: {Perimeter():F2}";
        }

        /// <inheritdoc/>
        public void Scale(double factor)
        {
            radius *= factor;
        }
    }

    /// <summary>A rectangle defined by its width and height.</summary>
    public class Rectangle : Shape, IScalable
    {
        private double width;
        private double height;

        /// <summary>
        /// Creates a rectangle with the given width and height.
        /// </summary>
        /// <param name="width">The width. Must be greater than zero.</param>
        /// <param name="height">The height. Must be greater than zero.</param>
        /// <exception cref="InvalidShapeException">Thrown when width or height is zero or negative.</exception>
        public Rectangle(double width, double height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new InvalidShapeException("Rectangle width and height must be greater than zero.");
            }
            this.width = width;
            this.height = height;
        }

        /// <inheritdoc/>
        public override double Area() => width * height;

        /// <inheritdoc/>
        public override double Perimeter() => 2 * (width + height);

        /// <inheritdoc/>
        public override string Describe()
        {
            return $"Rectangle | {width:F2}x{height:F2} | Area: {Area():F2} | Perimeter: {Perimeter():F2}";
        }

        /// <inheritdoc/>
        public void Scale(double factor)
        {
            width *= factor;
            height *= factor;
        }
    }

    /// <summary>A triangle defined by the lengths of its three sides.</summary>
    public class Triangle : Shape, IScalable
    {
        private double a;
        private double b;
        private double c;

        /// <summary>
        /// Creates a triangle with the given side lengths.
        /// </summary>
        /// <param name="a">Side a. Must be greater than zero.</param>
        /// <param name="b">Side b. Must be greater than zero.</param>
        /// <param name="c">Side c. Must be greater than zero.</param>
        /// <exception cref="InvalidShapeException">Thrown when any side is zero or negative.</exception>
        public Triangle(double a, double b, double c)
        {
            if (a <= 0 || b <= 0 || c <= 0)
            {
                throw new InvalidShapeException("Triangle sides must be greater than zero.");
            }
            this.a = a;
            this.b = b;
            this.c = c;
        }

        /// <summary>Calculates the area using Heron's formula.</summary>
        public override double Area()
        {
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        /// <inheritdoc/>
        public override double Perimeter() => a + b + c;

        /// <inheritdoc/>
        public override string Describe()
        {
            return $"Triangle | a={a:F2} b={b:F2} c={c:F2} | Area: {Area():F2} | Perimeter: {Perimeter():F2}";
        }

        /// <inheritdoc/>
        public void Scale(double factor)
        {
            a *= factor;
            b *= factor;
            c *= factor;
        }
    }

    /// <summary>
    /// Builds a list of shapes, prints a report through polymorphic Shape references,
    /// scales every shape, and prints the report again.
    /// </summary>
    public class ShapeHierarchy
    {
        /// <summary>
        /// Entry point. Runs the shape report before and after scaling, then demonstrates
        /// InvalidShapeException being thrown and caught for an invalid dimension.
        /// </summary>
        public static void Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            List<Shape> shapes = new List<Shape>
            {
                new Circle(5),
                new Rectangle(4, 6),
                new Triangle(3, 4, 5)
            };

            Console.WriteLine("=== Shape Report ===");
            foreach (Shape shape in shapes)
            {
                Console.WriteLine(shape.Describe());
            }

            Console.WriteLine("--- Scaling all shapes by 2.00 ---");
            foreach (Shape shape in shapes)
            {
                ((IScalable)shape).Scale(2.0);
            }

            foreach (Shape shape in shapes)
            {
                Console.WriteLine(shape.Describe());
            }

            try
            {
                Shape invalid = new Circle(-1);
            }
            catch (InvalidShapeException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }
}
