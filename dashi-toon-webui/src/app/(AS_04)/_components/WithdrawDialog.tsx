import React, { useState } from "react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { formatCurrency } from "@/utils";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { DialogTrigger } from "@radix-ui/react-dialog";
import { withdraw } from "@/utils/api/author-studio/revenue";
import { LoadingSpinner } from "@/components/ui/spinner";
import cx from "classnames";
import { FaCashRegister } from "react-icons/fa";
import { WalletMinimal } from "lucide-react";
import { toast } from "sonner";

export interface WithdrawDialogProps {
    balance: number;
    children?: React.ReactNode;
}

export default function WithdrawDialog({
    balance,
    children,
}: WithdrawDialogProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [paypalId, setPaypalId] = useState("");
    const [amount, setAmount] = useState("");
    const [withdrawAll, setWithdrawAll] = useState(false);
    const [isWithdrawing, setIsWithdrawing] = useState(false);

    const handleWithdraw = async () => {
        if (!paypalId) {
            toast.error("Vui lòng nhập ID PayPal của bạn");
            return;
        }

        const amountNum = Number(amount);
        if (!withdrawAll) {
            if (!amount) {
                toast.error("Vui lòng nhập số tiền cần rút");
                return;
            }

            if (!(0 < amountNum && amountNum <= balance)) {
                toast.error("Số tiền rút không được vượt quá số dư hiện tại");
                return;
            }
        }

        setIsWithdrawing(true);
        const [_, err] = await withdraw({
            paypalAccountId: paypalId,
            amount: withdrawAll ? balance : amountNum,
        });

        if (err) {
            toast.error("Không thể rút tiền, vui lòng thử lại sau");
            setIsWithdrawing(false);
            return;
        }

        toast.success(
            "Rút tiền thành công! Vui lòng kiểm tra tài khoản PayPal của bạn",
        );

        setTimeout(() => window.location.reload(), 1000);
    };

    return (
        <Dialog open={isOpen} onOpenChange={setIsOpen}>
            <DialogTrigger asChild>{children}</DialogTrigger>
            <DialogContent className="bg-neutral-800 sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle className="text-gray-100">
                        Rút tiền
                    </DialogTitle>
                    <DialogDescription className="text-gray-400">
                        Nhập thông tin PayPal của bạn để rút tiền. Số dư hiện
                        tại: {formatCurrency(balance || 0)}
                    </DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                    <div className="grid grid-cols-4 items-center gap-4">
                        <label
                            htmlFor="paypal-id"
                            className="mt-[8px] self-start text-right text-gray-300"
                        >
                            ID PayPal
                        </label>
                        <div className="col-span-3">
                            <Input
                                id="paypal-id"
                                value={paypalId}
                                onChange={(e) => setPaypalId(e.target.value)}
                                className="bg-neutral-700 text-gray-100"
                            />
                            <p className="col-span-4 mt-4 text-sm text-gray-400">
                                Để biết cách lấy ID PayPal của bạn, vui lòng xem{" "}
                                <Link
                                    href="https://www.paypal.com/vn/smarthelp/article/how-do-i-find-my-paypal-account-id-faq1694"
                                    className="text-blue-400 hover:underline"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                >
                                    hướng dẫn này
                                </Link>
                                .
                            </p>
                        </div>
                    </div>
                    <div className="grid grid-cols-4 items-center gap-4">
                        <label
                            htmlFor="amount"
                            className="text-right text-gray-300"
                        >
                            Số tiền
                        </label>
                        <Input
                            id="amount"
                            type="number"
                            value={amount}
                            onChange={(e) => setAmount(e.target.value)}
                            className="col-span-3 bg-neutral-700 text-gray-100"
                            disabled={withdrawAll}
                        />
                    </div>
                    <div className="flex items-center space-x-2">
                        <Checkbox
                            id="withdraw-all"
                            checked={withdrawAll}
                            onCheckedChange={(checked) =>
                                setWithdrawAll(checked as boolean)
                            }
                            className="border-gray-400"
                        />
                        <label
                            htmlFor="withdraw-all"
                            className="text-sm font-medium leading-none text-gray-300 peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                        >
                            Rút toàn bộ số dư
                        </label>
                    </div>
                </div>
                <DialogFooter>
                    <Button
                        type="submit"
                        onClick={handleWithdraw}
                        disabled={isWithdrawing}
                        className="flex gap-2 bg-blue-500 text-white hover:bg-blue-600"
                    >
                        {isWithdrawing ? (
                            <LoadingSpinner
                                className={cx({ hidden: !isWithdrawing })}
                            />
                        ) : (
                            <WalletMinimal />
                        )}
                        Xác nhận rút tiền
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
