import React, { useState, useEffect } from "react";

export default function CountdownTimer(props: { leftTime: number }) {
    const [time, setTime] = useState({
        days: 0,
        hours: 0,
        minutes: 0,
        seconds: 0,
    });
    const [timeLeft, setTimeLeft] = useState(props.leftTime);

    const calculateTimeLeft = (timeLeft: number) => {
        const seconds = Math.floor(timeLeft % 60);
        const minutes = Math.floor((timeLeft / 60) % 60);
        const hours = Math.floor((timeLeft / (60 * 60)) % 24);
        const days = Math.floor(timeLeft / (60 * 60 * 24));
        return { days, hours, minutes, seconds };
    };

    useEffect(() => {
        // Update the countdown every second
        const timer = setTimeout(() => {
            if (timeLeft >= 0) {
                setTimeLeft(timeLeft - 1);
                setTime(calculateTimeLeft(timeLeft));
            }
        }, 1000);

        return () => clearTimeout(timer);
    }, [timeLeft]);

    return (
        <div className="absolute bottom-3 flex w-full flex-col items-center justify-center gap-9 rounded-2xl bg-cover bg-center">
            <div className="count-down-main flex w-full items-start justify-center gap-1.5">
                <div className="timer">
                    <div className="flex min-w-[45px] flex-col items-center justify-center rounded-xl bg-black/25 backdrop-blur-sm">
                        <h3 className="countdown-element days font-manrope text-center text-2xl font-semibold text-white">
                            {time.days}
                        </h3>
                        <p className="mt-1 w-full text-center font-normal uppercase text-white">
                            ngày
                        </p>
                    </div>
                </div>

                <div className="timer">
                    <div className="flex min-w-[45px] flex-col items-center justify-center rounded-xl bg-black/25 backdrop-blur-sm">
                        <h3 className="countdown-element hours font-manrope text-center text-2xl font-semibold text-white">
                            {time.hours}
                        </h3>
                        <p className="mt-1 w-full text-center font-normal uppercase text-white">
                            giờ
                        </p>
                    </div>
                </div>

                <div className="timer">
                    <div className="flex min-w-[45px] flex-col items-center justify-center rounded-xl bg-black/25 backdrop-blur-sm">
                        <h3 className="countdown-element minutes font-manrope text-center text-2xl font-semibold text-white">
                            {time.minutes}
                        </h3>
                        <p className="mt-1 w-full text-center font-normal uppercase text-white">
                            phút
                        </p>
                    </div>
                </div>

                <div className="timer">
                    <div className="flex min-w-[45px] flex-col items-center justify-center rounded-xl bg-black/25 backdrop-blur-sm">
                        <h3 className="countdown-element seconds font-manrope text-center text-2xl font-semibold text-white">
                            {time.seconds}
                        </h3>
                        <p className="mt-1 w-full text-center font-normal uppercase text-white">
                            giây
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
