using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using PaymentSystem.Dto;
using Options = Iyzipay.Options;

namespace PaymentSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {

        private Options options = new()
        {
            ApiKey = "sandbox-tGEGZZQxKHQIyQ397sK9cfpLc1dET8K6",
            SecretKey = "sandbox-VeMVkfOoum6o79BewRvLLO1PMvCxX1nQ",
            BaseUrl = "https://sandbox-api.iyzipay.com"     // "https://api.iyzipay.com" (Canlıya bağlarken kullanmanız gereken BaseUrl)
        };

        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] IyzicoPayUserDto iyzicoPayUserDto)
        {
            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = iyzicoPayUserDto.Price;
            request.PaidPrice = iyzicoPayUserDto.Price;
            request.Currency = Currency.TRY.ToString();
            request.BasketId = "B67832";
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "http://localhost:44375/api/Payments/PayCallBack"; //canlıya taşırken kendi domainimizi yazmamız lazım

            List<int> enabledInstallments = new List<int>();
            enabledInstallments.Add(2);
            enabledInstallments.Add(3);
            enabledInstallments.Add(6);
            enabledInstallments.Add(9);
            request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer();
            buyer.Id = iyzicoPayUserDto.UserId;
            buyer.Name = iyzicoPayUserDto.UserName;
            buyer.Surname = iyzicoPayUserDto.UserSurname;
            buyer.GsmNumber = iyzicoPayUserDto.UserPhone;
            buyer.Email = iyzicoPayUserDto.UserEmail;
            buyer.IdentityNumber = "74300864791";
            buyer.RegistrationAddress = iyzicoPayUserDto.UserAddress;
            buyer.Ip = "85.34.78.112";
            buyer.City = iyzicoPayUserDto.UserCity;
            buyer.Country = "Turkey";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = iyzicoPayUserDto.UserName + " " + iyzicoPayUserDto.UserSurname;
            shippingAddress.City = iyzicoPayUserDto.UserCity;
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = iyzicoPayUserDto.UserAddress;
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = iyzicoPayUserDto.UserName + " " + iyzicoPayUserDto.UserSurname;
            billingAddress.City = iyzicoPayUserDto.UserCity;
            billingAddress.Country = "Turkey";
            billingAddress.Description = iyzicoPayUserDto.UserAddress;
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = iyzicoPayUserDto.UserReceivingPaymentId;
            firstBasketItem.Name = iyzicoPayUserDto.UserReceivingPayment + " ";
            firstBasketItem.Category1 = "Online Gym Cordinator";
            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = iyzicoPayUserDto.Price;
            basketItems.Add(firstBasketItem);
            request.BasketItems = basketItems;

            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);

            //Assert.AreEqual(Status.SUCCESS.ToString(), checkoutFormInitialize.Status);
            //Assert.AreEqual(Locale.TR.ToString(), checkoutFormInitialize.Locale);
            //Assert.AreEqual("123456789", checkoutFormInitialize.ConversationId);
            //Assert.IsNotNull(checkoutFormInitialize.SystemTime);
            //Assert.IsNull(checkoutFormInitialize.ErrorMessage);
            //Assert.IsNotNull(checkoutFormInitialize.CheckoutFormContent);
            //Assert.IsNotNull(checkoutFormInitialize.PaymentPageUrl);

            return Ok(new { Message = "Ödeme başarıyla gerçekleştirildi.", PaymentInfo = checkoutFormInitialize });
        }

        [HttpPost]
        public async Task<IActionResult> CancelPayment([FromBody] IyzicoPayUserDto id)
        {
            CreateCancelRequest request = new CreateCancelRequest();
            request.Locale = Locale.TR.ToString();
            request.PaymentId = id.PaymentId;
            request.Ip = "85.34.78.112";
            Cancel cancel = Cancel.Create(request, options);
            return Ok(cancel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBalance([FromQuery] IyzicoPayUserDto checkBalance)
        {
            RetrievePaymentRequest request = new RetrievePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                PaymentId = checkBalance.PaymentId
            };

            Payment payment = Payment.Retrieve(request, options);

            var balanceInfo = new
            {
                Price = payment.Price,
                PaidPrice = payment.PaidPrice
            };

            return Ok(balanceInfo);
        }

        [HttpPost]
        public async Task<IActionResult> PayCallBack()
        {
            RetrieveCheckoutFormRequest iyzicorequest = new RetrieveCheckoutFormRequest();
            iyzicorequest.ConversationId = "123456789";
            iyzicorequest.Token = HttpContext.Request.Form["token"];

            CheckoutForm checkoutForm = CheckoutForm.Retrieve(iyzicorequest, options);

            //Assert.AreEqual(Status.SUCCESS.ToString(), checkoutForm.Status);
            //Assert.AreEqual(Locale.TR.ToString(), checkoutForm.Locale);
            //Assert.AreEqual("123456789", checkoutForm.ConversationId);
            //Assert.IsNotNull(checkoutForm.SystemTime);
            //Assert.IsNull(checkoutForm.ErrorMessage);
            //Assert.IsNotNull(checkoutForm.Token);
            //Assert.IsNotNull(checkoutForm.PaymentId);

            if (checkoutForm.PaymentStatus.ToLower() == Status.FAILURE.ToString().ToLower() || checkoutForm.PaymentId == null)
            {
                //yazılan html kodları react native için hazır html yapısı döner
                string failContent = @"<!DOCTYPE html>
                            <html>
                            <head>
                                <title>Failed Payment</title>
                            </head>
                            <body>
                                <div>
                                    <script>
                                        (window['ReactNativeWebView'])?.postMessage('fail,-1');
                                        window['parent']?.postMessage(
                                            {
                                                type: 'fail',
                                                message: '-1',
                                            },
                                            '*'
                                        );
                                    </script>
                                </div>
                            </body>
                            </html>";

                return Content(failContent, "text/html");
            }

            else
            {

                string successForm = $@" <!DOCTYPE html>
                        <html>
                        <head>
                            <title>Successful Payment</title>
                        </head>
                        <body>
                            <div>
                                <script>
                                    (window['ReactNativeWebView'])?.postMessage('success,{checkoutForm.PaymentId}');
                                    window['parent']?.postMessage(
                                        {{
                                            type: 'success',
                                            message: '{checkoutForm.PaymentId}',
                                        }},
                                        '*'
                                    );
                                </script>
                            </div>
                        </body>
                        </html>
                        ";

                return Content(successForm, "text/html");
            }
        }
    }
}