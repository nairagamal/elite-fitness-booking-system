// Controllers/MockPaymentController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace elite.Controllers
{
    [ApiController]
    [Route("mock-payment")]
    [AllowAnonymous]
    public class MockPaymentController : ControllerBase
    {
        [HttpGet("checkout/{checkoutId}")]
        public IActionResult MockCheckoutPage(string checkoutId)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Mock Payment Gateway - Test</title>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            max-width: 500px; 
            margin: 50px auto; 
            padding: 20px; 
        }}
        .card {{ 
            border: 1px solid #ddd; 
            padding: 20px; 
            border-radius: 8px; 
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .btn {{ 
            padding: 10px 20px; 
            margin: 5px; 
            border: none; 
            border-radius: 4px; 
            cursor: pointer; 
            font-size: 16px;
        }}
        .success {{ 
            background: #28a745; 
            color: white; 
        }}
        .failure {{ 
            background: #dc3545; 
            color: white; 
        }}
        .info {{
            background: #17a2b8;
            color: white;
        }}
        h2 {{ color: #333; }}
        p {{ color: #666; }}
    </style>
</head>
<body>
    <div class=""card"">
        <h2>🔒 Mock Payment Gateway</h2>
        <p><strong>Checkout ID:</strong> {checkoutId}</p>
        <p>This is a simulated payment page for testing purposes only.</p>
        <p><strong>Amount:</strong> 100.00 SAR</p>
        
        <h3>Test Payment Scenarios:</h3>
        <button class=""btn success"" onclick=""processPayment('success')"">✅ Simulate Successful Payment</button>
        <button class=""btn failure"" onclick=""processPayment('failure')"">❌ Simulate Failed Payment</button>
        <button class=""btn info"" onclick=""window.close()"">🚫 Cancel Payment</button>
        
        <script>
            function processPayment(result) {{
                const paymentData = {{
                    orderId: 123,
                    amount: 100.00,
                    currency: 'SAR',
                    paymentMethod: 'mock',
                    customerEmail: 'test@example.com',
                    customerPhone: '+966500000000'
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
                        alert('✅ Payment Successful!\\\\nPayment ID: ' + data.paymentId);
                    }} else {{
                        alert('❌ Payment Failed!\\\\nReason: ' + data.message);
                    }}
                    
                    // Close window after short delay
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
        public IActionResult MockSuccessPage()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Payment Successful</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            max-width: 500px; 
            margin: 50px auto; 
            padding: 20px; 
            text-align: center;
        }
        .success { 
            color: #28a745; 
            font-size: 48px;
        }
        .card { 
            border: 1px solid #ddd; 
            padding: 30px; 
            border-radius: 8px; 
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""success"">✅</div>
        <h2>Payment Successful!</h2>
        <p>Your mock payment has been processed successfully.</p>
        <p>This is a test page - no actual payment was processed.</p>
        <button onclick=""window.close()"">Close Window</button>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        [HttpGet("failure")]
        public IActionResult MockFailurePage()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Payment Failed</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            max-width: 500px; 
            margin: 50px auto; 
            padding: 20px; 
            text-align: center;
        }
        .failure { 
            color: #dc3545; 
            font-size: 48px;
        }
        .card { 
            border: 1px solid #ddd; 
            padding: 30px; 
            border-radius: 8px; 
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""failure"">❌</div>
        <h2>Payment Failed</h2>
        <p>Your mock payment could not be processed.</p>
        <p>This is a test scenario - no actual payment was attempted.</p>
        <button onclick=""window.close()"">Close Window</button>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
}