/**
 * Atividade Cubo com dois buffers.
 * @author Filipe Santos Lima, Luiz Augusto Mendes Barbosa, Marcos Cabral Barbosa
 */

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

public class RotatingCube : GameWindow
{
	private int _vertexBufferObject;
	private int _uvBufferObject;
	private int _indexBufferObject;
	private Shader _shader;
	private Matrix4 _modelMatrix;
	private float _rotationSpeed = 1.5f;

	private readonly float[] _vertices =
	{
		-0.5f, -0.5f, -0.5f,
		 0.5f, -0.5f, -0.5f,
		 0.5f,  0.5f, -0.5f,
		-0.5f,  0.5f, -0.5f,
		-0.5f, -0.5f,  0.5f,
		 0.5f, -0.5f,  0.5f,
		 0.5f,  0.5f,  0.5f,
		-0.5f,  0.5f,  0.5f,
	};

	private readonly float[] _uvs =
	{
		0.0f, 0.0f,
		1.0f, 0.0f,
		1.0f, 1.0f,
		0.0f, 1.0f,
		0.0f, 0.0f,
		1.0f, 0.0f,
		1.0f, 1.0f,
		0.0f, 1.0f,
	};

	private readonly uint[] _indices =
	{
		0, 1, 2, 2, 3, 0,
		4, 5, 6, 6, 7, 4,
		0, 3, 7, 7, 4, 0,
		1, 2, 6, 6, 5, 1,
		3, 2, 6, 6, 7, 3,
		0, 1, 5, 5, 4, 0
	};

	public RotatingCube() : base(800, 600, GraphicsMode.Default, "Cubo Rotacional em 3D")
	{
		_modelMatrix = Matrix4.Identity;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		_shader = new Shader(VertexShaderSource, FragmentShaderSource);
		_shader.Use();

		_vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

		_uvBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, _uvs.Length * sizeof(float), _uvs, BufferUsageHint.StaticDraw);

		_indexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

		int vertexLocation = _shader.GetAttribLocation("aPosition");
		GL.EnableVertexAttribArray(vertexLocation);
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
		GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

		int uvLocation = _shader.GetAttribLocation("aTexCoord");
		GL.EnableVertexAttribArray(uvLocation);
		GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferObject);
		GL.VertexAttribPointer(uvLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

		GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		base.OnUpdateFrame(e);
		_modelMatrix *= Matrix4.CreateRotationY(_rotationSpeed * (float)e.Time);
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		_shader.Use();

		int modelLoc = _shader.GetUniformLocation("model");
		GL.UniformMatrix4(modelLoc, false, ref _modelMatrix);

		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
		GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

		SwapBuffers();
	}

	protected override void OnUnload(EventArgs e)
	{
		base.OnUnload(e);
		GL.DeleteBuffer(_vertexBufferObject);
		GL.DeleteBuffer(_uvBufferObject);
		GL.DeleteBuffer(_indexBufferObject);
		_shader.Dispose();
	}

	[STAThread]
	public static void Main()
	{
		using (var window = new RotatingCube())
		{
			window.Run(60.0);
		}
	}

	private const string VertexShaderSource = @"
    #version 330 core
    layout(location = 0) in vec3 aPosition;
    layout(location = 1) in vec2 aTexCoord;

    uniform mat4 model;

    out vec2 TexCoord;

    void main()
    {
        gl_Position = model * vec4(aPosition, 1.0);
        TexCoord = aTexCoord;
    }";

	private const string FragmentShaderSource = @"
    #version 330 core
    in vec2 TexCoord;
    out vec4 FragColor;

    void main()
    {
        FragColor = vec4(TexCoord, 0.0, 1.0);
    }";
}

public class Shader
{
	public int Handle;

	public Shader(string vertexShaderSource, string fragmentShaderSource)
	{
		int vertexShader = GL.CreateShader(ShaderType.VertexShader);
		GL.ShaderSource(vertexShader, vertexShaderSource);
		GL.CompileShader(vertexShader);
		CheckShaderCompileErrors(vertexShader, "VERTEX");

		int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fragmentShader, fragmentShaderSource);
		GL.CompileShader(fragmentShader);
		CheckShaderCompileErrors(fragmentShader, "FRAGMENT");

		Handle = GL.CreateProgram();
		GL.AttachShader(Handle, vertexShader);
		GL.AttachShader(Handle, fragmentShader);
		GL.LinkProgram(Handle);
		CheckProgramLinkErrors(Handle);

		GL.DeleteShader(vertexShader);
		GL.DeleteShader(fragmentShader);
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(Handle, attribName);
	}

	public int GetUniformLocation(string uniformName)
	{
		return GL.GetUniformLocation(Handle, uniformName);
	}

	public void Dispose()
	{
		GL.DeleteProgram(Handle);
	}

	private void CheckShaderCompileErrors(int shader, string type)
	{
		GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
		if (success == 0)
		{
			string infoLog = GL.GetShaderInfoLog(shader);
			throw new Exception($"{type} SHADER COMPILATION ERROR: {infoLog}");
		}
	}

	private void CheckProgramLinkErrors(int program)
	{
		GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
		if (success == 0)
		{
			string infoLog = GL.GetProgramInfoLog(program);
			throw new Exception($"PROGRAM LINKING ERROR: {infoLog}");
		}
	}
}
