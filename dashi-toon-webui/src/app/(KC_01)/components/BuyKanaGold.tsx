import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { getKanaPacks, KanaPack } from "@/utils/api/reader/kana";
import { Coins } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { PaymentMethodModal } from "./PaymentMethodModal";

export function BuyKanaGold() {
    const [kanaPacks, setKanaPacks] = useState<KanaPack[]>([]);
    const [selectedPack, setSelectedPack] = useState<KanaPack | null>(null);
    const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);

    useEffect(() => {
        fetchKanaPacks();
    }, []);

    const fetchKanaPacks = async () => {
        const [data, error] = await getKanaPacks();
        if (data) {
            setKanaPacks(data);
        } else {
            toast.error("Không thể tải thông tin gói Kana Gold", error);
        }
    };

    const handlePaymentProcessed = () => {
        setIsPaymentModalOpen(false);
        setSelectedPack(null);
        toast.success(
            "Giao dịch thành công! Trang sẽ được làm mới để cập nhật số dư Kana Gold của bạn.",
        );

        setTimeout(() => {
            window.location.reload();
        }, 2000);
    };

    const handleBuyClick = (pack: KanaPack) => {
        setSelectedPack(pack);
        setIsPaymentModalOpen(true);
    };

    return (
        <>
            <Card className="border-none bg-neutral-900 shadow-lg shadow-black/50 backdrop-blur-sm">
                <CardHeader className="border-b border-neutral-700 pb-4">
                    <CardTitle className="flex items-center text-2xl font-bold">
                        <Coins className="mr-3 h-8 w-8 flex-shrink-0 text-yellow-500" />
                        Kana Gold
                    </CardTitle>
                    <CardDescription>
                        Mua KanaGold để mở khoá chương mới.
                    </CardDescription>
                </CardHeader>
                <CardContent className="pt-6">
                    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
                        {kanaPacks.map((pack) => (
                            <div
                                key={pack.id}
                                className="flex flex-col justify-between rounded-xl border border-neutral-700/50 bg-gradient-to-br from-neutral-800 to-neutral-900 p-6 shadow-lg shadow-black/30 transition-all duration-300 hover:border-neutral-600 hover:shadow-xl"
                            >
                                <div className="mb-4">
                                    <div className="mb-3 flex items-center justify-center">
                                        <Coins className="h-12 w-12 text-yellow-500" />
                                    </div>
                                    <h3 className="mb-1 text-center text-3xl font-bold text-yellow-500">
                                        {pack.amount.toLocaleString()}
                                    </h3>
                                    <p className="text-center text-sm text-gray-400">
                                        Kana Gold
                                    </p>
                                </div>
                                <div className="text-center">
                                    <p className="mb-4 text-base font-semibold text-gray-300">
                                        {pack.price.amount.toLocaleString()}{" "}
                                        {pack.price.currency}
                                    </p>
                                    <Button
                                        className="w-full transform rounded-lg bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-3 font-semibold text-white shadow-md transition-all duration-300 hover:scale-105 hover:from-blue-600 hover:to-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:ring-opacity-50"
                                        onClick={() => handleBuyClick(pack)}
                                    >
                                        MUA NGAY
                                    </Button>
                                </div>
                            </div>
                        ))}
                    </div>
                </CardContent>
            </Card>
            <PaymentMethodModal
                isOpen={isPaymentModalOpen}
                onOpenChange={setIsPaymentModalOpen}
                selectedPack={selectedPack}
                onPaymentProcessed={handlePaymentProcessed}
            />
        </>
    );
}
