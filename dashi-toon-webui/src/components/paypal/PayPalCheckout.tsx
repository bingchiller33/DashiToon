import React, { useState } from "react";
import { PayPalScriptProvider, PayPalButtons } from "@paypal/react-paypal-js";

import * as env from "@/utils/env";
import { toast } from "sonner";

interface PayPalCheckoutProps {
    amount: string;
    currency: string;
    packId: string;
    onSuccess: (details: any) => void;
}

// function Message({ content }: { content: string }) {
//     return <p className="mt-2 text-center text-sm text-red-500">{content}</p>;
// }

export default function PayPalCheckout({
    currency,
    packId,
    onSuccess,
}: PayPalCheckoutProps) {
    const [message, setMessage] = useState<string>("");

    const initialOptions = {
        clientId: env.PAYPAL_CLIENT_ID,
        currency: currency,
        components: "buttons",
        locale: "vi_VN",
    };

    return (
        <div className="paypal-button-container">
            <PayPalScriptProvider options={initialOptions}>
                <PayPalButtons
                    style={{
                        shape: "pill",
                        layout: "vertical",
                        color: "gold",
                        label: "pay",
                    }}
                    createOrder={async () => {
                        try {
                            const response = await fetch(
                                `${env.getBackendHost()}/api/Subscriptions/kana-packs/${packId}/purchase`,
                                {
                                    credentials: "include",
                                    method: "POST",
                                    headers: {
                                        "Content-Type": "application/json",
                                    },
                                    body: JSON.stringify({
                                        packId: packId,
                                    }),
                                },
                            );
                            const orderData = await response.json();

                            if (orderData.id) {
                                return orderData.id;
                            } else {
                                const errorDetail = orderData?.details?.[0];
                                const errorMessage = errorDetail
                                    ? `${errorDetail.issue} ${errorDetail.description} (${orderData.debug_id})`
                                    : JSON.stringify(orderData);

                                throw new Error(errorMessage);
                            }
                        } catch (error) {
                            const errorMessage =
                                "Không thể bắt đầu thanh toán PayPal. Vui lòng thử lại sau.";
                            setMessage(errorMessage);
                            toast.error(errorMessage);
                        }
                    }}
                    onApprove={async (data, actions) => {
                        try {
                            const response = await fetch(
                                `${env.getBackendHost()}/api/Subscriptions/kana-packs/${data.orderID}/capture`,
                                {
                                    credentials: "include",
                                    method: "POST",
                                    headers: {
                                        "Content-Type": "application/json",
                                    },
                                },
                            );

                            const orderData = await response.json();

                            if (orderData.status === 4) {
                                if (
                                    orderData.purchaseUnits &&
                                    orderData.purchaseUnits.length > 0
                                ) {
                                    const firstPurchaseUnit =
                                        orderData.purchaseUnits[0];
                                    if (
                                        firstPurchaseUnit.payments &&
                                        firstPurchaseUnit.payments.captures
                                    ) {
                                        const capture =
                                            firstPurchaseUnit.payments
                                                .captures[0];
                                        if (capture) {
                                            const successMessage = `Giao dịch ${capture.status}: ${capture.id} thành công!`;
                                            setMessage(successMessage);
                                            toast.success(successMessage);
                                            onSuccess(orderData);
                                        } else {
                                            throw new Error(
                                                "Lỗi xử lý giao dịch",
                                            );
                                        }
                                    } else {
                                        throw new Error(
                                            "Trạng thái đơn hàng không hợp lệ",
                                        );
                                    }
                                }
                            }
                        } catch (error) {
                            const errorMessage =
                                "Xin lỗi, giao dịch của bạn không thể được xử lý. Vui lòng thử lại sau.";
                            setMessage(errorMessage);
                            toast.error(errorMessage);
                        }
                    }}
                    onError={(err) => {
                        const errorMessage =
                            "Đã xảy ra lỗi trong quá trình thanh toán. Vui lòng thử lại sau.";
                        setMessage(errorMessage);
                        toast.error(errorMessage);
                    }}
                />
            </PayPalScriptProvider>
            {/* <Message content={message} /> */}
        </div>
    );
}
