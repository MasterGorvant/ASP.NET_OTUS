using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System.Threading.Tasks;
using System;
using Xunit;
using AutoFixture;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.Core.Abstractions.Repositories;
using FluentAssertions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AutoFixture.Xunit2;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using System.Linq;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{


    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IRepository<Partner>> _mockRepository;
        private readonly PartnersController _controller;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            _fixture = new Fixture();
            
            // Игнорируем свойство Partner
            _fixture.Customize<PartnerPromoCodeLimit>(c => c.Without(x => x.Partner));
            // Игнорируем свойство PartnerLimits
            _fixture.Customize<Partner>(c => c.Without(x => x.PartnerLimits));
            _mockRepository = new();

            //Создание тестируемого объекта через AutoFixture
            _fixture.Inject<IRepository<Partner>>(_mockRepository.Object);
            _controller = _fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }

        //1. Если партнер не найден, то также нужно выдать ошибку 404;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerNotFound_Returns404()
        {
            var partnerId = Guid.NewGuid();
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partnerId)).ReturnsAsync((Partner)null);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partnerId, request);

            result.Should().BeOfType<NotFoundResult>();
        }

        //2. Если партнер заблокирован, то есть поле IsActive=false в классе Partner, то также нужно выдать ошибку 400;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsInactive_Returns400()
        {
            var partner = _fixture.Build<Partner>().With(p => p.IsActive, false).Create();
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        //3. Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал NumberIssuedPromoCodes, если лимит закончился, то количество не обнуляется;
        [Theory]
        [InlineData(null)]
        [InlineData("2024-10-01 00:00")]
        public async Task SetPartnerPromoCodeLimitAsync_SendValidRequest_SetsNewLimitAndResetsPromoCodes(string limitCloseDateStr)
        {
            DateTime? limitCloseDate = string.IsNullOrEmpty(limitCloseDateStr) ? null : DateTime.Parse(limitCloseDateStr);
            bool needResetPromocodes = limitCloseDate == null; // лимот активен, если у него нет даты сброса 

            var limit = _fixture.Build<PartnerPromoCodeLimit>().With(l => l.CancelDate, limitCloseDate).Create();
            var partner = _fixture.Build<Partner>()
                .With(p => p.IsActive, true)
                .With(p => p.NumberIssuedPromoCodes, 10)
                .With(p => p.PartnerLimits, new List<PartnerPromoCodeLimit>() { limit }).Create();
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>().With(r => r.Limit, 30).Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<CreatedAtActionResult>();
            if(needResetPromocodes)
            {
                partner.NumberIssuedPromoCodes.Should().Be(0);
            }
            else
            {
                partner.NumberIssuedPromoCodes.Should().Be(10);
            }
                
            partner.PartnerLimits.Should().ContainSingle(limit => limit.Limit == request.Limit && limit.CancelDate == null);
        }

        //4.При установке лимита нужно отключить предыдущий лимит;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_SendValidRequest_ResetsPreviousLimit()
        {
            var partner = _fixture.Build<Partner>().With(p => p.IsActive, true).Create();
            var previousLimit = _fixture.Build<PartnerPromoCodeLimit>().With(l => l.CancelDate, (DateTime?)null).Create();
            partner.PartnerLimits.Add(previousLimit);
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>().With(r => r.Limit, 30).Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<CreatedAtActionResult>();
            previousLimit.CancelDate.Should().NotBeNull();
        }

        //5.Лимит должен быть больше 0;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_LimitIsZeroOrNegative_Returns400()
        {
            var partner = _fixture.Build<Partner>().With(p => p.IsActive, true).Create();
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>().With(r => r.Limit, 0).Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
        }

        //6.Нужно убедиться, что сохранили новый лимит в базу данных
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_SendValidRequest_LimitIsSavedToDB()
        {
            var partner = _fixture.Build<Partner>().With(p => p.IsActive, true).Create();
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>().With(r => r.Limit, 30).Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<CreatedAtActionResult>();

            var savedPartner = await _mockRepository.Object.GetByIdAsync(partner.Id);
            var lastLimit = savedPartner.PartnerLimits.LastOrDefault();
            lastLimit.Should().NotBeNull();
            lastLimit.Limit.Should().Be(30);
        }
    }
}