using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LingoShift.Application.ApplicationServices;
using LingoShift.Application.DTOs;
using LingoShift.Application.Events;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.DomainServices;
using LingoShift.Domain.ValueObjects;
using Moq;
using Xunit;

namespace LingoShift.Tests.ApplicationServices
{
    public class TranslationApplicationServiceTests
    {
        private readonly Mock<ITranslationProvider> _mockTranslationProvider;
        private readonly Mock<IClipboardService> _mockClipboardService;
        private readonly Mock<IHotkeyService> _mockHotkeyService;
        private readonly Mock<LlmApplicationService> _mockLlmService;
        private readonly Mock<IPopupService> _mockPopupService;
        private readonly Mock<IDispatcherService> _mockDispatcherService;
        private readonly Mock<ISettingsService> _mockSettingsService;

        private readonly TranslationApplicationService _service;

        public TranslationApplicationServiceTests()
        {
            _mockTranslationProvider = new Mock<ITranslationProvider>();
            _mockClipboardService = new Mock<IClipboardService>();
            _mockHotkeyService = new Mock<IHotkeyService>();
            _mockLlmService = new Mock<LlmApplicationService>();
            _mockPopupService = new Mock<IPopupService>();
            _mockDispatcherService = new Mock<IDispatcherService>();
            _mockSettingsService = new Mock<ISettingsService>();

            _service = new TranslationApplicationService(
                _mockTranslationProvider.Object,
                _mockClipboardService.Object,
                _mockHotkeyService.Object,
                _mockLlmService.Object,
                _mockPopupService.Object,
                _mockDispatcherService.Object,
                _mockSettingsService.Object
            );
        }

        [Fact]
        public async Task RegisterSequencesAsync_ShouldRegisterAllSequences()
        {
            // Arrange
            var sequenceConfigs = new List<SequenceConfig>
            {
                new SequenceConfig { SequenceName = "Test1", Sequence = "<test1" },
                new SequenceConfig { SequenceName = "Test2", Sequence = "<test2" }
            };
            _mockSettingsService.Setup(s => s.GetSequenceConfigsAsync()).ReturnsAsync(sequenceConfigs);

            // Act
            await _service.RegisterSequencesAsync();

            // Assert
            _mockHotkeyService.Verify(h => h.RegisterSequence(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RegisterDefaultSequencesAsync_ShouldSetAndRegisterDefaultSequences()
        {
            // Act
            await _service.RegisterDefaultSequencesAsync();

            // Assert
            _mockSettingsService.Verify(s => s.SetSequenceConfigsAsync(It.IsAny<List<SequenceConfig>>()), Times.Once);
            _mockHotkeyService.Verify(h => h.RegisterSequence(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action>()), Times.AtLeast(1));
        }

        [Fact]
        public async Task ExecuteSequenceAction_ShouldTranslateAndNotifyWhenShowPopupIsTrue()
        {
            // Arrange
            var config = new SequenceConfig
            {
                SequenceName = "Test",
                Sequence = "<test",
                TargetLanguage = new Language("en", "English"),
                Action = SequenceAction.Translate,
                UseLLM = false,
                ShowPopup = true
            };
            _mockClipboardService.Setup(c => c.SelectAllAndCopyTextAsync()).ReturnsAsync("Test text<test");
            _mockTranslationProvider.Setup(t => t.TranslateAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Translated text");

            bool eventRaised = false;
            _service.TranslationCompleted += (sender, args) => eventRaised = true;

            // Act
            await _service.RegisterSequencesAsync();
            _mockHotkeyService.Raise(h => h.SequenceDetected += null, new SequenceDetectedEventArgs(config.SequenceName));

            // Assert
            _mockTranslationProvider.Verify(t => t.TranslateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.True(eventRaised);
        }

        [Fact]
        public async Task ExecuteSequenceAction_ShouldUseLlmWhenConfigured()
        {
            // Arrange
            var config = new SequenceConfig
            {
                SequenceName = "TestLLM",
                Sequence = "<testllm",
                TargetLanguage = new Language("it", "Italian"),
                Action = SequenceAction.Translate,
                UseLLM = true,
                ShowPopup = false
            };
            _mockClipboardService.Setup(c => c.SelectAllAndCopyTextAsync()).ReturnsAsync("Test text<testllm");
            _mockClipboardService.Setup(c => c.GetClipBoardTextAsync()).ReturnsAsync("Test text");
            _mockLlmService.Setup(l => l.GenerateResponse(It.IsAny<LlmRequestDto>())).ReturnsAsync(new LlmResponseDto { Content = "LLM translated text" });

            // Act
            await _service.RegisterSequencesAsync();
            _mockHotkeyService.Raise(h => h.SequenceDetected += null, new SequenceDetectedEventArgs(config.SequenceName));

            // Assert
            _mockLlmService.Verify(l => l.GenerateResponse(It.IsAny<LlmRequestDto>()), Times.Once);
        }
    }
}