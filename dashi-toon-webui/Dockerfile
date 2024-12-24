FROM node:20-alpine as builder

RUN apk add --no-cache libc6-compat
WORKDIR /app

ARG NEXT_PUBLIC_BACKEND_HOST=http://localhost:8080
ENV NEXT_PUBLIC_BACKEND_HOST $NEXT_PUBLIC_BACKEND_HOST
ARG NEXT_PUBLIC_SIGNALR_URL=http://localhost:8080/konichiwassup
ENV NEXT_PUBLIC_SIGNALR_URL $NEXT_PUBLIC_SIGNALR_URL

# Install dependencies based on the preferred package manager
COPY package.json package-lock.json ./
RUN  npm i


COPY . .

RUN npm run build

FROM node:20-alpine as runner
WORKDIR /app
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone .
COPY --from=builder /app/.next/static ./.next/static

# RUN chown -R root:root .
# RUN chmod -R 777 .



ENV NODE_ENV production
ENV PORT 3000
EXPOSE 3000

CMD HOSTNAME="0.0.0.0" node server.js