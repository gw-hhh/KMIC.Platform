# 测试用的 MinIO 镜像（MinioFixture 默认用它）。MinIO 社区版从 2025 年 10 月起不再发布官方镜像，所以从源码构建。
#   docker build -t kmic/minio:RELEASE.2025-10-15T17-29-55Z docker/minio
# 升级时同时改 MINIO_TAG、MINIO_REVISION（标签对应的提交，防止标签被改指向）和 MinioFixture 里的镜像名

FROM golang:1.24.8-bookworm AS build
ARG MINIO_TAG=RELEASE.2025-10-15T17-29-55Z
ARG MINIO_REVISION=9e49d5e7a648f00e26f2246f4dc28e6b07f8c84a
RUN git clone --branch "$MINIO_TAG" --depth 1 https://github.com/minio/minio /src \
 && test "$(git -C /src rev-parse HEAD)" = "$MINIO_REVISION"
WORKDIR /src
# 和上游发布构建一样，版本信息由 gen-ldflags.go 按标签生成，标为正式版而不是开发版
RUN CGO_ENABLED=0 go build -tags kqueue -trimpath --ldflags "$(MINIO_RELEASE=RELEASE go run buildscripts/gen-ldflags.go)" -o /out/minio

FROM debian:bookworm-slim
ARG MINIO_REVISION=9e49d5e7a648f00e26f2246f4dc28e6b07f8c84a
COPY --from=build /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
COPY --from=build /out/minio /usr/local/bin/minio
COPY --from=build /src/LICENSE /usr/share/doc/minio/LICENSE
LABEL org.opencontainers.image.source=https://github.com/minio/minio \
      org.opencontainers.image.revision=$MINIO_REVISION
RUN install -d -o 10001 -g 10001 /data /home/minio
ENV HOME=/home/minio
USER 10001:10001
EXPOSE 9000 9001
ENTRYPOINT ["/usr/local/bin/minio"]
