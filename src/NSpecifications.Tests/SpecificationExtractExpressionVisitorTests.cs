using System;
using System.Linq.Expressions;
using FluentAssertions;
using Xunit;

namespace NSpecifications.Tests
{
    public class SpecificationExtractExpressionVisitorTests
    {
        private string _comparedValue = "some";

        public string ComparedValue => "some";

        public Spec<string> Spec => new (s => s == "some");
        

        [Fact]
        public void ReplacerShouldReplaceLocalVariable()
        {
            // ARRANGE
            var spec = new Spec<string>(s => s == "some");
            Expression<Func<string, bool>> transformedExpression = x => x.Is(spec);

            Expression<Func<string, bool>> expectedExpression = x => x == "some";

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            // ASSERT

            newExp.Should().NotBeNull();
            newExp!.ToString().Should().Be(expectedExpression.ToString());
        }

        [Fact]
        public void ReplacerShouldReplaceProperty()
        {
            // ARRANGE
            Expression<Func<string, bool>> transformedExpression = x => x.Is(Spec);

            Expression<Func<string, bool>> expectedExpression = x => x == "some";

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            // ASSERT

            newExp.Should().NotBeNull();
            newExp!.ToString().Should().Be(expectedExpression.ToString());
        }
        
        [Fact]
        public void ReplacerShouldReplaceConstructorWithConstant()
        {
            // Подобные тесты должны в том числе проверять, что функция Is или другие были заменены,
            // то есть что их не осталось в выражении. Пока не знаю, как это сделать наиболее правильным способом.
            // Без этого тесты, которые в конце проверяют результат скомпилирванных функций, бесполезны :(

            // ARRANGE
            Expression<Func<string, bool>> transformedExpression = x => x.Is(new TestSpec("some"));

            Expression<Func<string, bool>> expectedExpression = x => x == "some";
            var expectedFunc = expectedExpression.Compile();
            var expectedResult = expectedFunc("some");

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            var newFunc = ((Expression<Func<string, bool>>?)newExp)?.Compile();

            var newResult = newFunc?.Invoke("some");

            // ASSERT

            newResult.Should().NotBeNull();
            newResult.Should().Be(expectedResult);
        }

        [Fact]
        public void ReplacerShouldReplaceConstructorWithLocalVariable()
        {
            // ARRANGE
            var comparedValue = "some";
            Expression<Func<string, bool>> transformedExpression = x => x.Is(new TestSpec(comparedValue));

            Expression<Func<string, bool>> expectedExpression = x => x == "some";
            var expectedFunc = expectedExpression.Compile();
            var expectedResult = expectedFunc("some");

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            var newFunc = ((Expression<Func<string, bool>>?)newExp)?.Compile();

            var newResult = newFunc?.Invoke("some");

            // ASSERT

            newResult.Should().NotBeNull();
            newResult.Should().Be(expectedResult);
        }

        [Fact]
        public void ReplacerShouldReplaceConstructorWithField()
        {
            // ARRANGE
            Expression<Func<string, bool>> transformedExpression = x => x.Is(new TestSpec(_comparedValue));

            Expression<Func<string, bool>> expectedExpression = x => x == "some";
            var expectedFunc = expectedExpression.Compile();
            var expectedResult = expectedFunc("some");

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            var newFunc = ((Expression<Func<string, bool>>?)newExp)?.Compile();

            var newResult = newFunc?.Invoke("some");

            // ASSERT

            newResult.Should().NotBeNull();
            newResult.Should().Be(expectedResult);
        }

        [Fact]
        public void ReplacerShouldReplaceConstructorWithProperty()
        {
            // ARRANGE
            Expression<Func<string, bool>> transformedExpression = x => x.Is(new TestSpec(ComparedValue));

            Expression<Func<string, bool>> expectedExpression = x => x == "some";
            var expectedFunc = expectedExpression.Compile();
            var expectedResult = expectedFunc("some");

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            var newFunc = ((Expression<Func<string, bool>>?)newExp)?.Compile();

            var newResult = newFunc?.Invoke("some");

            // ASSERT

            newResult.Should().NotBeNull();
            newResult.Should().Be(expectedResult);
        }

        [Fact]
        public void ReplacerShouldReplaceConstructorWithMethod()
        {
            // ARRANGE
            Expression<Func<string, bool>> transformedExpression = x => x.Is(new TestSpec(GetComparedValue()));

            Expression<Func<string, bool>> expectedExpression = x => x == "some";
            var expectedFunc = expectedExpression.Compile();
            var expectedResult = expectedFunc("some");

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            var newFunc = ((Expression<Func<string, bool>>?)newExp)?.Compile();

            var newResult = newFunc?.Invoke("some");

            // ASSERT

            newResult.Should().NotBeNull();
            newResult.Should().Be(expectedResult);
        }

        [Fact]
        public void ReplacerShouldReplaceConstructorWithLambda()
        {
            // ARRANGE
            Expression<Func<string, bool>> transformedExpression = x => x.Is(new Spec<string>(s => s == "some"));

            Expression<Func<string, bool>> expectedExpression = x => x == "some";
            var expectedFunc = expectedExpression.Compile();
            var expectedResult = expectedFunc("some");

            var visitor = new SpecificationReplaceExpressionVisitor();

            // ACT

            var newExp = visitor.Visit(transformedExpression);

            var newFunc = ((Expression<Func<string, bool>>?)newExp)?.Compile();

            var newResult = newFunc?.Invoke("some");

            // ASSERT

            newResult.Should().NotBeNull();
            newResult.Should().Be(expectedResult);
        }

        private string GetComparedValue()
        {
            return "some";
        }
    }

    public class TestSpec : Spec<string>
    {
        /// <inheritdoc />
        public TestSpec(string value) : base(x => x == value)
        {
        }
    }
}