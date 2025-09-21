namespace src.Templates
{
    public static class PaymentSuccessTemplate
    {
        public static string Generate(string amount)
        {
            return $@"
                <div
                    style='
                        margin: 0;
                        font-family: ""Poppins"", sans-serif;
                        background: #ffffff;
                        font-size: 14px;
                    '
                >
                    <div
                        style='
                            max-width: 680px;
                            margin: 0 auto;
                            padding: 45px 30px 60px;
                            background: #f4f7ff;
                            font-size: 14px;
                            color: #434343;
                        '
                    >
                        <main>
                            <div
                                style='
                                    margin: 0;
                                    margin-top: 70px;
                                    padding: 92px 30px 115px;
                                    background: #ffffff;
                                    border-radius: 30px;
                                    text-align: center;
                                '
                            >
                                <div style='width: 100%; max-width: 489px; margin: 0 auto;'>
                                    <h1
                                        style='
                                            margin: 0;
                                            font-size: 24px;
                                            font-weight: 600;
                                            color: #1f1f1f;
                                        '
                                    >
                                        Payment Successful 🎉
                                    </h1>
                                    <p
                                        style='
                                            margin: 0;
                                            margin-top: 17px;
                                            font-weight: 500;
                                            letter-spacing: 0.56px;
                                            color: #333333;
                                        '
                                    >
                                        Thank you for your payment.<br />
                                        The transaction has been completed successfully.
                                    </p>
                                    <p
                                        style='
                                            margin: 0;
                                            margin-top: 30px;
                                            font-size: 20px;
                                            font-weight: 600;
                                            color: #2c7a7b;
                                        '
                                    >
                                        Amount Paid: {amount:C}
                                    </p>
                                    <p
                                        style='
                                            margin: 0;
                                            margin-top: 40px;
                                            font-size: 14px;
                                            color: #666666;
                                        '
                                    >
                                        If you have any questions, please contact our support team.
                                    </p>
                                </div>
                            </div>
                        </main>
                    </div>
                </div>
            ";
        }
    }
}
