import { Coins } from "lucide-react";
import { toast } from "sonner";

import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { KanaPack } from "@/utils/api/reader/kana";
import PayPalCheckout from "@/components/paypal/PayPalCheckout";
import { useState } from "react";
import * as env from "@/utils/env";

interface PaymentMethodModalProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    selectedPack: KanaPack | null;
    onPaymentProcessed: () => void;
}

export function PaymentMethodModal({
    isOpen,
    onOpenChange,
    selectedPack,
    onPaymentProcessed,
}: PaymentMethodModalProps) {
    // const [selectedPaymentMethod, setSelectedPaymentMethod] = useState<
    //     string | null
    // >(null);

    // const handlePaymentMethodSelect = (method: string) => {
    //     setSelectedPaymentMethod(method);
    // };

    // const handleProceedPayment = async () => {
    //     if (selectedPaymentMethod && selectedPack) {
    //         try {
    //             const response = await fetch(
    //                 `${env.getBackendHost()}/api/Subscriptions/kana-packs/${selectedPack.id}/purchase`,
    //                 {
    //                     credentials: "include",
    //                     method: "POST",
    //                     headers: {
    //                         "Content-Type": "application/json",
    //                     },
    //                     body: JSON.stringify({
    //                         packId: selectedPack.id,
    //                         paymentMethod: selectedPaymentMethod,
    //                         returnUrl: window.location.href,
    //                     }),
    //                 },
    //             );

    //             const data = await response.json();

    //             window.location.href = data;
    //         } catch (error) {
    //             console.error("Error processing payment:", error);
    //             toast.error("Có lỗi xảy ra. Vui lòng thử lại sau.");
    //         }

    //         onPaymentProcessed();
    //         setSelectedPaymentMethod(null);
    //     } else {
    //         toast.error("Vui lòng chọn phương thức thanh toán");
    //     }
    // };

    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="bg-neutral-900 text-white sm:max-w-[480px]">
                <DialogHeader>
                    <DialogTitle className="text-xl font-bold">
                        Mua thêm Kana Gold
                    </DialogTitle>
                </DialogHeader>
                <div className="py-3">
                    <h2 className="pb-1">Lựa chọn của bạn</h2>
                    <div className="mb-6 flex items-center justify-between">
                        <div className="flex items-center">
                            <Coins className="mr-3 h-8 w-8 flex-shrink-0 text-yellow-500" />
                            <div className="flex flex-col">
                                <span className="flex items-center text-2xl font-semibold text-yellow-500">
                                    {selectedPack?.amount.toLocaleString()}
                                </span>
                                <span className="text-sm text-gray-400">
                                    Kana Gold
                                </span>
                            </div>
                        </div>
                        <div className="text-2xl font-bold">
                            {selectedPack?.price.amount.toLocaleString()}{" "}
                            {selectedPack?.price.currency}
                        </div>
                    </div>
                    <h3 className="mb-2 text-lg font-medium">
                        Phương thức thanh toán
                    </h3>
                    <div className="mb-4 rounded-lg bg-white px-2 pt-2 shadow-lg">
                        {selectedPack && (
                            <PayPalCheckout
                                amount={selectedPack.price.amount.toString()}
                                currency="USD"
                                packId={selectedPack.id}
                                onSuccess={onPaymentProcessed}
                            />
                        )}
                    </div>
                    <div className="mb-1 text-xs text-gray-400">
                        Trải nghiệm thanh toán an toàn được cung cấp bởi PayPal
                        và Canada. Thông tin phương thức thanh toán không được
                        lưu trữ trên DashitOON.
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}
