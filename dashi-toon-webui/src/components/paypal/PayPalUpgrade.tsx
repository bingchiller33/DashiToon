import React, { useState } from "react";
import { PayPalScriptProvider, PayPalButtons } from "@paypal/react-paypal-js";

import * as env from "@/utils/env";
import { toast } from "sonner";

interface PayPalCheckoutProps {
    amount: string;
    currency?: string;
    subId: string;
    tierId: string;
    onSuccess: (details: any) => void;
}

function Message({ content }: { content: string }) {
    return <p>{content}</p>;
}

export default function PayPalUpgrade({
    currency,
    subId,
    tierId,
    onSuccess,
}: PayPalCheckoutProps) {
    const [message, setMessage] = useState<string>("");

    const initialOptions = {
        clientId: env.PAYPAL_CLIENT_ID,
        currency: currency ?? "USD",
        components: "buttons",
    };

    return (
        <div className="paypal-button-container">
            <PayPalScriptProvider options={initialOptions}>
                <PayPalButtons
                    style={{
                        shape: "rect",
                        layout: "vertical",
                        label: "paypal",
                    }}
                    createOrder={async () => {
                        console.log("pgkid: ", subId);
                        try {
                            const response = await fetch(
                                `${env.getBackendHost()}/api/Users/subscriptions/${subId}/upgrade`,
                                {
                                    credentials: "include",
                                    method: "PUT",
                                    headers: {
                                        "Content-Type": "application/json",
                                    },
                                    body: JSON.stringify({
                                        subscriptionId: subId,
                                        tierId: tierId,
                                    }),
                                },
                            );
                            console.log("response: ", response);
                            const orderData = await response.json();
                            console.log("orderData: ", orderData);

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
                            console.error(error);
                            setMessage(
                                `Could not initiate PayPal Checkout...${error}`,
                            );
                        }
                    }}
                    onApprove={async (data, actions) => {
                        try {
                            const response = await fetch(
                                `${env.getBackendHost()}/api/Users/subscriptions/${data.orderID}/upgrade/capture`,
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
                        setMessage("PayPal Checkout error");
                        console.error(err);
                    }}
                />
            </PayPalScriptProvider>
            <Message content={message} />
        </div>
    );
}
