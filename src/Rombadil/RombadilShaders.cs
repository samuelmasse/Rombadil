namespace Rombadil;

public static class RombadilShaders
{
    public const string Frag =
        """
        #version 330

        in vec2 fragTexCoord;

        out vec4 outColor;

        uniform sampler2D texSampler;

        void main() {
            outColor = texture(texSampler, fragTexCoord);
        }     
        """;

    public const string Vert =
        """
        #version 330
        
        layout(location = 0) in vec2 inPosition;
        layout(location = 1) in vec2 inTexCoord;

        out vec2 fragTexCoord;

        void main() {
            fragTexCoord = inTexCoord;
            gl_Position = vec4(inPosition, 0, 1);
        }   
        """;
}
