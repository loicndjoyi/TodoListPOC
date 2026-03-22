using FluentAssertions;
using TodoApp.Core;

namespace TodoApp.UnitTests;

public class TodoItemTests
{
    [Fact]
    public void Constructor_ValidTitle_CreatesItemWithCorrectDefaults()
    {
        // Arrange
        var title = "Learn Angular 21";

        // Act
        var todo = new TodoItem(title);

        // Assert
        todo.Id.Should().NotBeEmpty();
        todo.Title.Should().Be(title);
        todo.IsCompleted.Should().BeFalse();
        todo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        todo.CompletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_NullOrWhitespaceTitle_ThrowsArgumentException(string invalidTitle)
    {
        // Act
        Action act = () => new TodoItem(invalidTitle);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Title cannot be empty*");
    }

    [Fact]
    public void Constructor_TwoInstances_GeneratesUniqueIds()
    {
        // Act
        var todo1 = new TodoItem("Task 1");
        var todo2 = new TodoItem("Task 2");

        // Assert
        todo1.Id.Should().NotBe(todo2.Id);
    }

    [Fact]
    public void Complete_IncompleteTodo_SetsIsCompletedAndCompletedAt()
    {
        // Arrange
        var todo = new TodoItem("Task");

        // Act
        todo.Complete();

        // Assert
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().NotBeNull();
        todo.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_AlreadyCompleted_IsIdempotent()
    {
        // Arrange
        var todo = new TodoItem("Task");
        todo.Complete();
        var initialCompletedAt = todo.CompletedAt;

        // Act — calling Complete() a second time should be a no-op
        todo.Complete();

        // Assert — CompletedAt must be the exact same value as before the second call
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().Be(initialCompletedAt);
    }

    [Fact]
    public void Uncomplete_CompletedTodo_ResetsIsCompletedAndCompletedAt()
    {
        // Arrange
        var todo = new TodoItem("Task");
        todo.Complete();

        // Act
        todo.Uncomplete();

        // Assert
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Uncomplete_AlreadyIncomplete_IsIdempotent()
    {
        // Arrange
        var todo = new TodoItem("Task");
        
        // Act
        todo.Uncomplete();

        // Assert
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateTitle_ValidTitle_ChangesTitle()
    {
        // Arrange
        var todo = new TodoItem("Initial Title");
        var newTitle = "Updated Title";

        // Act
        todo.UpdateTitle(newTitle);

        // Assert
        todo.Title.Should().Be(newTitle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateTitle_NullOrWhitespaceTitle_ThrowsArgumentException(string invalidTitle)
    {
        // Arrange
        var todo = new TodoItem("Initial Title");

        // Act
        Action act = () => todo.UpdateTitle(invalidTitle);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Title cannot be empty*");
        
        // Ensure title wasn't accidentally changed
        todo.Title.Should().Be("Initial Title");
    }
}
