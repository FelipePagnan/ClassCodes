using DevFreela.Core.Entities;
using DevFreela.Core.Enums;
using DevFreela.UnitTests.Fakes;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevFreela.UnitTests.Core
{
    public class ProjectTests
    {
        [Fact]
        public void ProjectIsCreated_Start_Success()
        {
            // Arrange
            // var project = new Project("Projeto A", "Descrição do Projeto", 1, 2, 10000);

            var project = FakeDataHelper.CreateFakeProject();

            // Act
            project.Start();

            // Assert
            Assert.Equal(ProjectStatusEnum.InProgress, project.Status);
            Assert.NotNull(project.StartedAt);

            project.StartedAt.Should().NotBeNull();
            project.Status.Should().Be(ProjectStatusEnum.InProgress);

            Assert.True(project.Status == ProjectStatusEnum.InProgress);
            Assert.False(project.StartedAt is null);
        }

        [Fact]
        public void ProjectIsInInvalidState_Start_ThrowsException()
        {
            // Arrange
            // var project = new Project("Projeto A", "Descrição do Projeto", 1, 2, 10000);
            var project = FakeDataHelper.CreateFakeProject();

            project.Start();

            // Act + Assert
            Action? start = project.Start;

            var exception = Assert.Throws<InvalidOperationException>(start);
            Assert.Equal(Project.INVALID_STATE_MESSAGE, exception.Message);

            start.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(Project.INVALID_STATE_MESSAGE);
        }
    }
}
