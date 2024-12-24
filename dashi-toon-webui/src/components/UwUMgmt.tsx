/* eslint-disable @next/next/no-img-element */
import React, { ReactNode, useState } from "react";
import {
    Drawer,
    DrawerClose,
    DrawerContent,
    DrawerDescription,
    DrawerFooter,
    DrawerHeader,
    DrawerTitle,
    DrawerTrigger,
} from "@/components/ui/drawer";
import { Button } from "./ui/button";
import { Switch } from "./ui/switch";
import { Label } from "./ui/label";
import useUwU from "@/hooks/useUwU";
import { Input } from "./ui/input";
import { useLocalStorage } from "usehooks-ts";
import { toast } from "sonner";

export interface UwUMgmtProps {
    children: ReactNode;
}

export default function UwUMgmt({ children }: UwUMgmtProps) {
    const [allowManagement, setAllowMangement] = useUwU("うをう");
    const [dizz, setDizz] = useUwU("dizz");
    const [dl, setDl] = useUwU("dl");
    const [rr, setRR] = useUwU("rr");
    const [enable, setEnable] = useUwU();
    const [analitycs, setAnalytics] = useUwU("amalytics");
    const [dc, setDc] = useLocalStorage("dizzChance", 0.1, {
        initializeWithValue: false,
    });
    const [lastClickTime, setLastClickTime] = useState(0);
    const [noClick, setNoClick] = useState(0);

    const handleClick = () => {
        const now = Date.now();
        if (now - lastClickTime < 1000) {
            setNoClick(noClick + 1);
        } else {
            setNoClick(1);
        }
        if (noClick === 10 && !allowManagement) {
            setAllowMangement(true);
            toast.success("うをうはあなたを愛しています❤️");
        }
        setLastClickTime(now);
    };

    if (!allowManagement) {
        return <div onClick={handleClick}>{children}</div>;
    }

    return (
        <Drawer
            onOpenChange={() =>
                localStorage.setItem("uwu_ver", new Date().toString())
            }
        >
            <DrawerTrigger asChild>{children}</DrawerTrigger>
            <DrawerContent className="container">
                <DrawerHeader>
                    <DrawerTitle>UwU?</DrawerTitle>
                    <DrawerDescription>
                        Nothing here! &gt; UwU &lt;
                    </DrawerDescription>
                </DrawerHeader>
                <div className="space-y-4">
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Switch
                            checked={enable}
                            onCheckedChange={(e) => setEnable(e)}
                        />
                        <Label>Enable?</Label>
                    </div>
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Switch
                            checked={analitycs}
                            onCheckedChange={(e) => setAnalytics(e)}
                        />
                        <Label>Analytics?</Label>
                    </div>
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Switch
                            checked={rr}
                            onCheckedChange={(e) => setRR(e)}
                        />
                        <Label>rr?</Label>
                    </div>
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Switch
                            checked={dizz}
                            onCheckedChange={(e) => setDizz(e)}
                        />
                        <Label>Dizz?</Label>
                    </div>
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Switch
                            checked={dl}
                            onCheckedChange={(e) => setDl(e)}
                        />
                        <Label>Dizz Long?</Label>
                    </div>
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Input
                            type="number"
                            placeholder="Dizz Chance"
                            min={0}
                            max={100}
                            value={dc * 100}
                            onChange={(e) =>
                                setDc(Number.parseFloat(e.target.value) / 100)
                            }
                        />
                    </div>
                    <div
                        className="uwu flex items-center space-x-2"
                        style={{ animationDelay: `${Math.random() * 1000}ms` }}
                    >
                        <Switch
                            checked={allowManagement}
                            onCheckedChange={(e) => setAllowMangement(e)}
                        />
                        <Label>再世なら?</Label>
                    </div>
                </div>
                <DrawerFooter>
                    <DrawerClose>
                        <Button variant="outline">Close</Button>
                    </DrawerClose>
                </DrawerFooter>

                <img src="/images/uwu.svg" className="uwu absolute" alt="uwu" />
            </DrawerContent>
        </Drawer>
    );
}
