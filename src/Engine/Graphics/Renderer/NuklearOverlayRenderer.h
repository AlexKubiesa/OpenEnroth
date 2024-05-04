#pragma once

#include <Engine/Graphics/Renderer/OpenGLShader.h>
#include <Library/Geometry/Size.h>

#include <memory>

struct nk_state;
struct nk_context;

struct nk_tex_font {
    uint32_t texid;
    struct nk_font *font;
};

class GraphicsImage;

class NuklearOverlayRenderer {
 public:
    NuklearOverlayRenderer();
    ~NuklearOverlayRenderer();
    void render(nk_context *context, const Sizei &outputPresent, bool useOGLES, int *drawCalls);
    void reloadShaders(bool useOGLES);

 private:
    void _initialize(nk_context *context);
    bool _createDevice();
    struct nk_tex_font* _loadFont(const char *font_path, size_t font_size);
    void _cleanup();

    std::unique_ptr<nk_state> _state;
    bool _useOGLES = false;
    OpenGLShader _shader;
    nk_tex_font *_defaultFont = nullptr;
};
