// Controllers/MockGatewayController.cs
using elite.MockGateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elite.Controllers
{
    [ApiController]
    [Route("mock-gateway/{gatewayName}")]
    [AllowAnonymous]
    public class MockGatewayController : ControllerBase
    {
        private readonly IMockGatewayFactory _gatewayFactory;
        private readonly ILogger<MockGatewayController> _logger;

        public MockGatewayController(IMockGatewayFactory gatewayFactory, ILogger<MockGatewayController> logger)
        {
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        [HttpGet("checkout/{checkoutId}")]
        public IActionResult MockCheckoutPage(string gatewayName, string checkoutId)
        {
            var gateway = _gatewayFactory.GetGateway(gatewayName);

            var gatewayDisplayName = gatewayName.ToLower() switch
            {
                "tabby" => "Tabby - Buy Now, Pay Later",
                "tamara" => "Tamara - Shop Now, Pay Later",
                "creditcard" => "Credit Card Payment",
                "mock" => "Mock Payment Gateway",
                _ => "Payment Gateway"
            };

            var gatewayDescription = gatewayName.ToLower() switch
            {
                "tabby" => "Test Tabby's installment payment system. Split your payment into 4, 6, or 12 months.",
                "tamara" => "Test Tamara's pay later service. Pay in 30 days or split into 3 installments.",
                "creditcard" => "Test credit card payment with simulated card validation and 3D Secure.",
                "mock" => "Generic test payment gateway for development.",
                _ => "Test payment gateway"
            };

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{gatewayDisplayName} - Test</title>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            max-width: 600px; 
            margin: 50px auto; 
            padding: 20px; 
        }}
        .card {{ 
            border: 1px solid #ddd; 
            padding: 30px; 
            border-radius: 12px; 
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }}
        .btn {{ 
            padding: 12px 24px; 
            margin: 10px 5px; 
            border: none; 
            border-radius: 6px; 
            cursor: pointer; 
            font-size: 16px;
            font-weight: 600;
            transition: transform 0.2s;
        }}
        .btn:hover {{ transform: translateY(-2px); }}
        .success {{ 
            background: #10B981; 
            color: white; 
        }}
        .failure {{ 
            background: #EF4444; 
            color: white; 
        }}
        .info {{
            background: #3B82F6;
            color: white;
        }}
        .cancel {{
            background: #6B7280;
            color: white;
        }}
        h2 {{ color: white; margin-bottom: 10px; }}
        h3 {{ color: white; margin-top: 25px; }}
        p {{ color: rgba(255,255,255,0.9); line-height: 1.6; }}
        .test-cards {{ 
            background: rgba(255,255,255,0.1); 
            padding: 15px; 
            border-radius: 8px; 
            margin: 20px 0;
        }}
        .test-cards h4 {{ margin-top: 0; color: white; }}
        .test-cards ul {{ margin: 10px 0; padding-left: 20px; }}
        .test-cards li {{ color: rgba(255,255,255,0.9); margin: 5px 0; }}
        .gateway-badge {{
            display: inline-block;
            background: rgba(255,255,255,0.2);
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 14px;
            margin-bottom: 15px;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""gateway-badge"">{gatewayDisplayName}</div>
        <h2>🔒 {gatewayDisplayName} - Test Mode</h2>
        <p><strong>Checkout ID:</strong> {checkoutId}</p>
        <p>{gatewayDescription}</p>
        <p><strong>Test Amount:</strong> 100.00 SAR</p>
        
        <div class=""test-cards"">
            <h4>💳 Test Payment Scenarios:</h4>
            <button class=""btn success"" onclick=""processPayment('success')"">✅ Simulate Successful Payment</button>
            <button class=""btn failure"" onclick=""processPayment('failure')"">❌ Simulate Failed Payment</button>
            <button class=""btn cancel"" onclick=""window.close()"">🚫 Cancel Payment</button>
        </div>

        <script>
            function processPayment(result) {{
                const paymentData = {{
                    orderId: 123,
                    amount: 100.00,
                    currency: 'SAR',
                    paymentMethod: '{gatewayName}',
                    customerEmail: 'test@example.com',
                    customerPhone: '+966500000000',
                    metadata: {{ test_result: result }}
                }};

                fetch('/api/payment/process?checkoutId={checkoutId}', {{
                    method: 'POST',
                    headers: {{ 
                        'Content-Type': 'application/json'
                    }},
                    body: JSON.stringify(paymentData)
                }})
                .then(response => response.json())
                .then(data => {{
                    if (data.success) {{
                        alert('✅ Payment Successful!\\nGateway: {gatewayDisplayName}\\nPayment ID: ' + data.paymentId + '\\nStatus: ' + data.status);
                    }} else {{
                        alert('❌ Payment Failed!\\nGateway: {gatewayDisplayName}\\nReason: ' + data.message);
                    }}
                    
                    setTimeout(() => {{
                        window.close();
                    }}, 2000);
                }})
                .catch(error => {{
                    alert('💥 Error processing payment: ' + error.message);
                }});
            }}
        </script>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        [HttpGet("success")]
        public IActionResult MockSuccessPage(string gatewayName)
        {
            var gatewayDisplayName = gatewayName.ToLower() switch
            {
                "tabby" => "Tabby",
                "tamara" => "Tamara",
                "creditcard" => "Credit Card",
                _ => "Payment"
            };

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{gatewayDisplayName} Payment Successful</title>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            max-width: 500px; 
            margin: 50px auto; 
            padding: 20px; 
            text-align: center;
        }}
        .success {{ 
            color: #10B981; 
            font-size: 64px;
            margin-bottom: 20px;
        }}
        .card {{ 
            border: 1px solid #10B981; 
            padding: 40px; 
            border-radius: 12px; 
            background: #ECFDF5;
        }}
        h2 {{ color: #065F46; margin-bottom: 15px; }}
        p {{ color: #047857; line-height: 1.6; margin: 10px 0; }}
        .gateway-info {{
            background: #D1FAE5;
            padding: 10px;
            border-radius: 6px;
            margin: 20px 0;
            color: #065F46;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""success"">✅</div>
        <h2>{gatewayDisplayName} Payment Successful!</h2>
        <div class=""gateway-info"">
            <strong>Gateway:</strong> {gatewayDisplayName}<br>
            <strong>Status:</strong> Completed<br>
            <strong>Mode:</strong> Test
        </div>
        <p>Your {gatewayDisplayName.ToLower()} payment has been processed successfully.</p>
        <p>This is a test simulation - no actual payment was processed.</p>
        <button onclick=""window.close()"" style='padding: 10px 20px; background: #10B981; color: white; border: none; border-radius: 6px; cursor: pointer;'>Close Window</button>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        [HttpGet("failure")]
        public IActionResult MockFailurePage(string gatewayName)
        {
            var gatewayDisplayName = gatewayName.ToLower() switch
            {
                "tabby" => "Tabby",
                "tamara" => "Tamara",
                "creditcard" => "Credit Card",
                _ => "Payment"
            };

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{gatewayDisplayName} Payment Failed</title>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            max-width: 500px; 
            margin: 50px auto; 
            padding: 20px; 
            text-align: center;
        }}
        .failure {{ 
            color: #EF4444; 
            font-size: 64px;
            margin-bottom: 20px;
        }}
        .card {{ 
            border: 1px solid #FCA5A5; 
            padding: 40px; 
            border-radius: 12px; 
            background: #FEF2F2;
        }}
        h2 {{ color: #991B1B; margin-bottom: 15px; }}
        p {{ color: #DC2626; line-height: 1.6; margin: 10px 0; }}
        .gateway-info {{
            background: #FEE2E2;
            padding: 10px;
            border-radius: 6px;
            margin: 20px 0;
            color: #991B1B;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""failure"">❌</div>
        <h2>{gatewayDisplayName} Payment Failed</h2>
        <div class=""gateway-info"">
            <strong>Gateway:</strong> {gatewayDisplayName}<br>
            <strong>Status:</strong> Failed<br>
            <strong>Mode:</strong> Test
        </div>
        <p>Your {gatewayDisplayName.ToLower()} payment could not be processed.</p>
        <p>This is a test scenario - no actual payment was attempted.</p>
        <button onclick=""window.close()"" style='padding: 10px 20px; background: #EF4444; color: white; border: none; border-radius: 6px; cursor: pointer;'>Close Window</button>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
}